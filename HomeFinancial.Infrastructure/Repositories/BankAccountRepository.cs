using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с банковскими счетами
/// </summary>
public sealed class BankAccountRepository : GenericRepository<BankAccount>, IBankAccountRepository
{
    private readonly ILogger _logger;

    public BankAccountRepository(ApplicationDbContext dbContext, ILogger<BankAccountRepository> logger)
        : base(dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<BankAccount> GetOrCreateAsync(int bankId, string accountId, string accountType, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(accountId, nameof(accountId));
        ArgumentNullException.ThrowIfNull(accountType, nameof(accountType));
        
        var account = await DbSet
            .FirstOrDefaultAsync(a => a.BankId == bankId && a.AccountId == accountId, ct);
        
        if (account != null)
        {
            return account;
        }
        
        _logger.LogInformation("Создание нового счёта: BankId={BankId}, AccountId={AccountId}, Type={Type}", 
            bankId, accountId, accountType);
        
        var newAccount = new BankAccount
        {
            BankId = bankId,
            AccountId = accountId,
            AccountType = accountType
        };
        
        return await CreateAsync(newAccount, ct);
    }
}
