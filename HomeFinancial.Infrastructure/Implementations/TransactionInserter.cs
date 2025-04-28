using System.Collections.Immutable;
using HomeFinancial.Application.Dtos;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace HomeFinancial.Infrastructure.Implementations;

public class TransactionInserter : ITransactionInserter
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger _logger;

    public TransactionInserter(ApplicationDbContext dbContext, ILogger<TransactionInserter> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<(int InsertedCount, int SkippedDuplicateCount)> BulkInsertCopyAsync(
        IList<TransactionInsertDto> transactions,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(transactions);

        var existingFitIds = await GetExistingFitIdsAsync(transactions, cancellationToken);

        foreach (var fitId in existingFitIds)
        {
            _logger.LogWarning("Транзакция с FitId={FitId} уже существует", fitId);
        }

        // Оставляем только новые DTO
        var newItems = transactions
            .Where(t => !existingFitIds.Contains(t.FitId))
            .ToList();

        if (newItems.Count == 0)
        {
            _logger.LogInformation("Новых транзакций для вставки через COPY не найдено.");
            return (0, existingFitIds.Count);
        }

        // Открываем соединение через EF Core
        var conn = (NpgsqlConnection) _dbContext.Database.GetDbConnection();
        await _dbContext.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            // Начинаем бинарный COPY для максимальной скорости
            await using var writer = await conn.BeginBinaryImportAsync(
                "COPY file_transactions " +
                "(file_id, fit_id, date, amount, description, category_id) " +
                "FROM STDIN (FORMAT BINARY)",
                cancellationToken);

            foreach (var t in newItems)
            {
                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(t.FileId, NpgsqlTypes.NpgsqlDbType.Integer, cancellationToken);
                await writer.WriteAsync(t.FitId,       NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(t.Date.ToUniversalTime(), NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                await writer.WriteAsync(t.Amount,      NpgsqlTypes.NpgsqlDbType.Numeric, cancellationToken);
                await writer.WriteAsync(t.Description, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(t.CategoryId,  NpgsqlTypes.NpgsqlDbType.Integer, cancellationToken);
            }

            await writer.CompleteAsync(cancellationToken);

            _logger.LogInformation("Бинарный COPY завершён. Вставлено {Count} транзакций.", newItems.Count);

            return (newItems.Count, existingFitIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при бинарном COPY в BulkInsertCopyAsync");
            throw;
        }
        finally
        {
            await _dbContext.Database.CloseConnectionAsync();
        }
    }        
    
    /// <summary>
    /// Получает список существующих FIT-ID из базы, сравнивая с переданным списком DTO
    /// </summary>
    /// <param name="transactions">Список DTO, для которых нужно найти существующие FIT-ID</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сет существующих FIT-ID</returns>
    private async Task<HashSet<string>> GetExistingFitIdsAsync(IList<TransactionInsertDto> transactions, CancellationToken cancellationToken)
    {
        var fitIds = transactions.Select(t => t.FitId).ToImmutableHashSet();

        return await _dbContext.FileTransactions
            .Where(t => fitIds.Contains(t.FitId))
            .Select(t => t.FitId)
            .ToHashSetAsync(cancellationToken);
    }
}