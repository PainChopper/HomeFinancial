using HomeFinancial.Application.Common.Abstractions;
using HomeFinancial.Infrastructure.Persistence;

namespace HomeFinancial.Infrastructure.Database;

public class DatabaseHealthChecker(ApplicationDbContext context) : IDatabaseHealthChecker
{
    public async Task<bool> CheckDatabaseHealthAsync(CancellationToken cancellationToken = default)
    {
        return await context.Database.CanConnectAsync(cancellationToken);
    }
}