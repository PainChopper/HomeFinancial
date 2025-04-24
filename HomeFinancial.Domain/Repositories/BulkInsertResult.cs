namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Результат пакетной вставки транзакций
/// </summary>
public struct BulkInsertResult
{
    /// <summary>
    /// Количество реально вставленных транзакций
    /// </summary>
    public int InsertedCount { get; init; }
    /// <summary>
    /// Количество пропущенных дубликатов
    /// </summary>
    public int SkippedDuplicateCount { get; init; }
}