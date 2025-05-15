using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Категория финансовой транзакции
/// </summary>
public class EntryCategory : Entity
{
    /// <summary>
    /// Название категории
    /// </summary>
    public required string Name { get; init; }
}
