using HomeFinancial.Application.Dtos;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Npgsql;

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
    public async Task<(int Inserted, int Duplicates)> BulkInsertCopyAsync(
        IList<TransactionInsertDto> transactions,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(transactions);

        var existingFitIds = await GetExistingFitIdsAsync(
            transactions.Select(t => t.FitId).ToArray(), 
            cancellationToken);

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

        await using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
        await conn.OpenAsync(cancellationToken);

        const string sql = """
                           COPY file_transactions
                           (file_id, fit_id, date, amount, description, category_id)
                           FROM STDIN (FORMAT BINARY)
                           """;
        try
        {
            await using var writer = await conn.BeginBinaryImportAsync(sql, cancellationToken);

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
    }

    /// <summary>
    /// Получает список существующих FIT-ID из базы, сравнивая с переданным списком DTO
    /// </summary>
    /// <param name="fitIds"></param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сет существующих FIT-ID</returns>
    private async Task<HashSet<string>> GetExistingFitIdsAsync(string[] fitIds, CancellationToken cancellationToken)
    {
        var result = new HashSet<string>();

        if (fitIds.Length == 0)
            return result;

        const string sql = """
                           SELECT fit_id
                           FROM file_transactions
                           WHERE fit_id = ANY (@fit_ids)
                           """;

        await using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("fit_ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, fitIds);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }
}