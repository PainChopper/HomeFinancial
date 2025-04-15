namespace HomeFinancial.Domain.Common;

/// <summary>
/// Базовый интерфейс для всех сущностей домена
/// </summary>
public interface IEntity
{
    int Id { get; set; }
}
