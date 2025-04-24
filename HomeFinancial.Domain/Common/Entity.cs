namespace HomeFinancial.Domain.Common;

/// <summary>
/// Базовый класс для всех сущностей домена
/// </summary>
public abstract record Entity : IEntity
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public int Id { get; set; }
}
