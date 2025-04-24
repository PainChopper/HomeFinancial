namespace HomeFinancial.Application.Common.Abstractions;

public interface IDatabaseHealthChecker
{
    Task<bool> CheckDatabaseHealthAsync(CancellationToken cancellationToken = default);
}