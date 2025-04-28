using System.Diagnostics;
using Npgsql;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Infrastructure.Utils;

public class RetryPolicyHelper
{
    private readonly ILogger<RetryPolicyHelper> _logger;

    public RetryPolicyHelper(ILogger<RetryPolicyHelper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Выполняет заданную операцию с повторными попытками при некоторых ошибках PostgreSQL.
    /// </summary>
    /// <typeparam name="T">Тип результата операции.</typeparam>
    /// <param name="operation">Асинхронная операция, которую нужно выполнить с повторами.</param>
    /// <returns>Результат успешного выполнения операции.</returns>
    /// <exception cref="RetryLimitExceededException">
    /// Выбрасывается, если операция не завершилась успешно из-за превышения лимита попыток или времени. 
    /// В исключение включается список всех ошибок, возникших при повторных попытках.
    /// </exception>
    public async Task<T> RetryAsync<T>(Func<Task<T>> operation)
    {
        ArgumentNullException.ThrowIfNull(operation, nameof(operation));

        var exceptions = new List<Exception>();
        // Таймер для отслеживания общего времени выполнения всех попыток
        var stopwatch = Stopwatch.StartNew();

        // Политика повторов для транзиентных ошибок (ошибки сериализации, дедлоки, ошибки соединения)
        var transientErrorsPolicy = Policy
            .Handle<PostgresException>(ex =>
                ex.SqlState == "40001"    // serialization_failure
                || ex.SqlState == "40P01" // deadlock_detected
                || (ex.SqlState?.StartsWith("08") ?? false) // сбой соединения (class 08 - Connection Exception)
            ).WaitAndRetryAsync(
                sleepDurations: Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromMilliseconds(200), retryCount: 5),
                onRetry: (exception, delay, retryAttempt, context) =>
                {
                    // Логируем информацию о неудачной попытке (на русском)
                    _logger.LogWarning("Попытка #{RetryAttempt} завершилась ошибкой SQLSTATE={SqlState}: {ErrorMessage}. Повтор через {Delay}.", retryAttempt, (exception as PostgresException)?.SqlState, exception.Message, delay);
                    exceptions.Add(exception);
                    // Проверяем, не вышли ли за пределы 30 секунд общего времени
                    if (stopwatch.Elapsed >= TimeSpan.FromSeconds(30))
                    {
                        throw new OperationCanceledException("Превышено общее время выполнения попыток");
                    }
                }
            );

        // Политика одного повтора при отмене запроса (SQLSTATE 57014)
        var cancellationPolicy = Policy
            .Handle<PostgresException>(ex => ex.SqlState == "57014")  // операция отменена (query_canceled)
            .WaitAndRetryAsync(
                retryCount: 1,
                sleepDurationProvider: _ => TimeSpan.FromSeconds(5),   // повтор через 5 секунд
                onRetry: (exception, delay, retryAttempt, context) =>
                {
                    _logger.LogWarning("Запрос был отменён (SQLSTATE 57014). Повторная попытка через {Delay}.", delay);
                    exceptions.Add(exception);
                    if (stopwatch.Elapsed >= TimeSpan.FromSeconds(30))
                    {
                        throw new OperationCanceledException("Превышено общее время выполнения попыток");
                    }
                }
            );

        // Комбинируем политики: сначала обрабатывается отмена запроса, затем транзиентные ошибки
        var combinedPolicy = Policy.WrapAsync(cancellationPolicy, transientErrorsPolicy);

        T result;
        try
        {
            // Выполняем операцию внутри комбинированной политики повторов
            result = await combinedPolicy.ExecuteAsync(operation);
        }
        catch (OperationCanceledException)
        {
            // Превышено общее время выполнения всех попыток
            throw new RetryLimitExceededException("Превышено максимальное время выполнения операции (с учётом повторов).", exceptions);
        }
        catch (Exception ex)
        {
            // Операция окончательно не удалась после всех повторов
            exceptions.Add(ex);
            if (ex is PostgresException)
            {
                throw new RetryLimitExceededException("Превышено максимальное количество попыток выполнения операции.", exceptions);
            }
            throw; // Пробросить неожиданные исключения (не связанные с PostgreSQL) без обёртки
        }

        return result;
    }
}

// Кастомное исключение, содержащее информацию о всех ошибках в ходе повторов
public class RetryLimitExceededException : AggregateException
{
    /// <summary>Сообщение об ошибке и список внутренних исключений при повторных попытках.</summary>
    public RetryLimitExceededException(string message, IEnumerable<Exception> innerExceptions)
        : base(message, innerExceptions)
    {
    }
}
