namespace HomeFinancial.Application.Common;

/// <summary>
/// Провайдер, возвращающий системное время
/// </summary>
public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
