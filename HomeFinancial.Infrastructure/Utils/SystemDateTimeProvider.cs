using HomeFinancial.Application.Common;

namespace HomeFinancial.Infrastructure.Utils;

/// <summary>
/// Провайдер, возвращающий системное время
/// </summary>
public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
