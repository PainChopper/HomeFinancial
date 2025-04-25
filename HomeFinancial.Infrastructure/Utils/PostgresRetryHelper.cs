using Npgsql;
using Polly;

namespace HomeFinancial.Infrastructure.Utils;

public static class PostgresRetryHelper
{
     // Метод, выполняющий заданную операцию с повторными попытками при определённых ошибках PostgreSQL.
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation, 
        CancellationToken cancellationToken)
    {
        // Политика повторов для транзиентных ошибок (сериализация, дедлок, проблемы соединения).
        var retryPolicyTransient = Policy
            .Handle<PostgresException>(ex =>
            {
                // Определяем, подходит ли ошибка под наши критерии:
                var sqlState = ex.SqlState;
                return sqlState == "40001"        // ошибка сериализации транзакции
                    || sqlState == "40P01"        // дедлок
                    || (sqlState?.StartsWith("08") ?? false); // сбой соединения (08XXX)
            })
            .WaitAndRetryAsync(
                retryCount: 5,   // до 5 попыток
                sleepDurationProvider: attempt =>
                {
                    // Экспоненциальная задержка с джиттером.
                    // Например: 1^2, 2^2, 4^2,... секунд + случайный миллисекундный разброс.
                    var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, 100));
                    return baseDelay + jitter;
                },
                onRetry: (exception, delay, attempt, context) =>
                {
                    // Логируем информацию о повторной попытке (на русском):
                    if (exception is PostgresException pex)
                    {
                        Console.WriteLine(
                            $"Предупреждение: ошибка SQLSTATE={pex.SqlState} ({pex.Message}). " + 
                            $"Попытка повторения {attempt}-я из 5, следующая через {delay.TotalSeconds:F1} с.");
                    }
                    else
                    {
                        Console.WriteLine(
                            $"Предупреждение: исключение {exception.GetType().Name}: {exception.Message}. " + 
                            $"Попытка повторения {attempt}-я из 5, следующая через {delay.TotalSeconds:F1} с.");
                    }
                });

        // Политика повторов для таймаута/отмены запроса (57014) – 1 дополнительная попытка через 5 секунд.
        var retryPolicyTimeout = Policy
            .Handle<PostgresException>(ex => ex.SqlState == "57014")
            .WaitAndRetryAsync(
                retryCount: 1,   // максимум 1 повтор
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(5),
                onRetry: (exception, delay, attempt, context) =>
                {
                    // Логируем информацию о повторе после таймаута (на русском):
                    Console.WriteLine("Запрос отменён или прерван (SQLSTATE 57014). " +
                                      "Повторная попытка через 5 секунд...");
                });

        // Объединяем политики через PolicyWrap: первой (внешней) идёт политика для таймаута, 
        // а внутренняя – для остальных транзиентных ошибок.
        var retryPolicyWrap = Policy.WrapAsync(retryPolicyTimeout, retryPolicyTransient);

        // Выполнение операции с применением комбинированной политики.
        // Polly самостоятельно вызовет делегат повторно при перехваченных ошибках.
        return await retryPolicyWrap.ExecuteAsync(ct => operation(ct), cancellationToken);
    }

}