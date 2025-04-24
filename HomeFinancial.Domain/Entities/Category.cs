using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Категория финансовой транзакции
/// </summary>
public record Category : Entity
{
    /// <summary>
    /// Название категории
    /// </summary>
    public required string Name { get; init; }
}
