using HomeFinancial.Application.Dtos;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Application.UseCases.ImportOfxFile;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace HomeFinancial.Infrastructure.Repositories;

public class TransactionInserter : ITransactionInserter
{
    private readonly ILogger _logger;
    private readonly ConnectionStrings _connectionStrings;

    public TransactionInserter(ILogger<TransactionInserter> logger, ConnectionStrings connectionStrings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionStrings = connectionStrings ?? throw new ArgumentNullException(nameof(connectionStrings));
    }

    /// <inheritdoc/>
    public async Task<BulkInsertResult> BulkInsertCopyAsync(
        IList<TransactionInsertDto> transactions,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(transactions);

        var existingFitIds = await GetExistingFitIdsAsync(
            transactions.Select(t => t.FitId).ToArray(), 
            ct);

        foreach (var fitId in existingFitIds)
        {
            _logger.LogWarning("Транзакция с Id={Id} уже существует", fitId);
        }

        // Оставляем только новые DTO
        var newItems = transactions
            .Where(t => !existingFitIds.Contains(t.FitId))
            .ToList();

        if (newItems.Count == 0)
        {
            _logger.LogInformation("Новых транзакций для вставки через COPY не найдено.");
            return new BulkInsertResult(0, existingFitIds.Count);
        }

        await using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
        await conn.OpenAsync(ct);

        const string sql = """
                           COPY statement_entries
                           (file_id, fit_id, date, amount, description, category_id, bank_account_id)
                           FROM STDIN (FORMAT BINARY)
                           """;
        try
        {
            await using var writer = await conn.BeginBinaryImportAsync(sql, ct);

            foreach (var t in newItems)
            {
                await writer.StartRowAsync(ct);
                await writer.WriteAsync(t.FileId, NpgsqlDbType.Integer, ct);
                await writer.WriteAsync(t.FitId,       NpgsqlDbType.Text, ct);
                await writer.WriteAsync(t.Date.ToUniversalTime(), NpgsqlDbType.TimestampTz, ct);
                await writer.WriteAsync(t.Amount,      NpgsqlDbType.Numeric, ct);
                await writer.WriteAsync(t.Description, NpgsqlDbType.Text, ct);
                await writer.WriteAsync(t.CategoryId,  NpgsqlDbType.Integer, ct);
                await writer.WriteAsync(t.BankAccountId,   NpgsqlDbType.Integer, ct);
            }

            await writer.CompleteAsync(ct);

            _logger.LogInformation("Бинарный COPY завершён. Вставлено {Count} транзакций.", newItems.Count);

            return new BulkInsertResult(newItems.Count, existingFitIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при бинарном COPY в BulkInsertCopyAsync");
            throw;
        }
    }

    /// <summary>
    /// Получает список существующих FIT-ID из базы, сравнивая с переданным списком DTO
    /// </summary>
    /// <param name="fitIds"></param>
    /// <param name="ct">Токен отмены операции</param>
    /// <returns>Сет существующих FIT-ID</returns>
    private async Task<HashSet<string>> GetExistingFitIdsAsync(string[] fitIds, CancellationToken ct)
    {
        var result = new HashSet<string>();

        if (fitIds.Length == 0)
            return result;

        const string sql = """
                           SELECT fit_id
                           FROM statement_entries
                           WHERE fit_id = ANY (@fit_ids)
                           """;

        await using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        cmd.Parameters.AddWithValue("fit_ids", NpgsqlDbType.Array | NpgsqlDbType.Text, fitIds);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }
}