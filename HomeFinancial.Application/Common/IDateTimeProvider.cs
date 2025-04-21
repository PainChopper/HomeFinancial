namespace HomeFinancial.Application.Common;

/// <summary>
/// Провайдер текущей даты и времени (для тестируемости и абстракции)
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Текущее время в UTC
    /// </summary>
    DateTime UtcNow { get; }
}
