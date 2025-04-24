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
    public async Task<BulkInsertResult> BulkInsertCopyAsync(IList<BankTransaction> transactions, CancellationToken cancellationToken)
    {
        var existingFitIds = await GetExistingFitIds(transactions, cancellationToken);

        foreach (var fitId in existingFitIds)
        {
            logger.LogWarning("Уже существует транзакция с FitId={FitId}", fitId);
        }

        // Оставляем только те транзакции, которых нет в базе
        var newTransactions = transactions
            .Where(t => !existingFitIds.Contains(t.FitId))
            .ToList();

        if (newTransactions.Count == 0)
        {
            logger.LogInformation("Нет новых транзакций для вставки через COPY.");
            return new BulkInsertResult { InsertedCount = 0, SkippedDuplicateCount = existingFitIds.Count };
        }

        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        var wasClosed = conn.State == System.Data.ConnectionState.Closed;
        if (wasClosed)
            await conn.OpenAsync(cancellationToken);

        try
        {
            // COPY для вашей схемы: bank_transactions (imported_file_id, fit_id, date, amount, description, category_id)
            await using (var writer = await conn.BeginTextImportAsync(
                             "COPY bank_transactions (imported_file_id, fit_id, date, amount, description, category_id) FROM STDIN (FORMAT CSV)",
                             cancellationToken))
            {
                foreach (var t in newTransactions)
                {
                    var line = string.Join(",",
                        t.ImportedFileId,
                        EscapeCsv(t.FitId),
                        t.Date.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        t.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        EscapeCsv(t.Description),
                        t.CategoryId?.ToString() ?? ""
                    ) + "\n";
                    await writer.WriteAsync(line);
                }
            }
            logger.LogInformation("COPY завершён. Вставлено {Count} транзакций.", newTransactions.Count);
            return new BulkInsertResult { InsertedCount = newTransactions.Count, SkippedDuplicateCount = existingFitIds.Count };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при выполнении COPY для BulkInsertCopyAsync");
            throw;
        }
        finally
        {
            if (wasClosed)
                await conn.CloseAsync();
        }
    }


    // Для экранирования строк в CSV
    private static string EscapeCsv(string value) =>
        string.IsNullOrEmpty(value) ? "" : $"\"{value.Replace("\"", "\"\"")}\"";
   
    /// <summary>
    /// Получает список существующих FIT-ID из базы, сравнивая с переданным списком транзакций
    /// </summary>
    /// <param name="transactions">Список транзакций, для которых нужно найти существующие FIT-ID</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список существующих FIT-ID</returns>
    private async Task<List<string>> GetExistingFitIds(IList<BankTransaction> transactions, CancellationToken cancellationToken)
    {
        var fitIds = transactions.Select(t => t.FitId).ToList();

        // Получаем список существующих FIT-ID одним запросом
        var existingFitIds = await dbContext.BankTransactions
            .Where(t => fitIds.Contains(t.FitId))
            .Select(t => t.FitId)
            .ToListAsync(cancellationToken);
        return existingFitIds;
    }
}
