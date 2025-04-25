using HomeFinancial.Application.Common;

namespace HomeFinancial.Infrastructure.Implementations;

/// <summary>
/// Провайдер, возвращающий системное время
/// </summary>
public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
