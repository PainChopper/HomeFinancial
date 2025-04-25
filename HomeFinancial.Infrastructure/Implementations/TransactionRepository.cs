using System.Collections.Immutable;
using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace HomeFinancial.Infrastructure.Implementations;

/// <summary>
/// Репозиторий для работы с банковскими транзакциями
/// </summary>
public class TransactionRepository(
    ApplicationDbContext dbContext, ILogger<TransactionRepository> logger)
    : ITransactionRepository
{
    /// <inheritdoc/>
    public async Task<(int InsertedCount, int SkippedDuplicateCount)> BulkInsertCopyAsync(
        IList<BankTransaction> transactions,
        CancellationToken cancellationToken)
    {
        // Проверяем, что список транзакций не null
        ArgumentNullException.ThrowIfNull(transactions);

        // Получаем множество существующих FitId (для O(1)-проверок)
        var existingFitIds = await GetExistingFitIdsAsync(transactions, cancellationToken);

        // Логируем все найденные дубликаты
        foreach (var fitId in existingFitIds)
        {
            logger.LogWarning("Транзакция с FitId={FitId} уже существует", fitId);
        }

        // Оставляем только новые транзакции
        var newTransactions = transactions
            .Where(t => !existingFitIds.Contains(t.FitId))
            .ToList();

        if (newTransactions.Count == 0)
        {
            logger.LogInformation("Новых транзакций для вставки через COPY не найдено.");
            return (0, existingFitIds.Count);
        }

        // Открываем соединение через EF Core
        var conn = (NpgsqlConnection) dbContext.Database.GetDbConnection();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            // Начинаем бинарный COPY для максимальной скорости
            await using var writer = await conn.BeginBinaryImportAsync(
                "COPY bank_transactions " +
                "(imported_file_id, fit_id, date, amount, description, category_id) " +
                "FROM STDIN (FORMAT BINARY)",
                cancellationToken);

            foreach (var t in newTransactions)
            {
                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(t.ImportedFileId, NpgsqlTypes.NpgsqlDbType.Integer, cancellationToken);
                await writer.WriteAsync(t.FitId,       NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(t.Date.ToUniversalTime(), NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                await writer.WriteAsync(t.Amount,      NpgsqlTypes.NpgsqlDbType.Numeric, cancellationToken);
                await writer.WriteAsync(t.Description, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(t.CategoryId,  NpgsqlTypes.NpgsqlDbType.Integer, cancellationToken);
            }

            await writer.CompleteAsync(cancellationToken);

            logger.LogInformation("Бинарный COPY завершён. Вставлено {Count} транзакций.", newTransactions.Count);

            return (newTransactions.Count, existingFitIds.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ошибка при бинарном COPY в BulkInsertCopyAsync");
            throw;
        }
        finally
        {
            // Всегда закрываем соединение
            await dbContext.Database.CloseConnectionAsync();
        }
    }




    /// <summary>
    /// Получает список существующих FIT-ID из базы, сравнивая с переданным списком транзакций
    /// </summary>
    /// <param name="transactions">Список транзакций, для которых нужно найти существующие FIT-ID</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сет существующих FIT-ID</returns>
    private async Task<HashSet<string>> GetExistingFitIdsAsync(IList<BankTransaction> transactions, CancellationToken cancellationToken)
    {
        var fitIds = 
            transactions.Select(t => t.FitId)
                .ToImmutableHashSet();

        return await dbContext.BankTransactions
            .Where(t => fitIds.Contains(t.FitId))
            .Select(t => t.FitId)
            .ToHashSetAsync(cancellationToken: cancellationToken);
    }
}
