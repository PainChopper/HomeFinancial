using HomeFinancial.Domain.Services;

namespace HomeFinancial.Infrastructure.Implementations
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
