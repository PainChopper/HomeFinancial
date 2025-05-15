using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с банками
/// </summary>
public sealed class BankRepository : GenericRepository<Bank>, IBankRepository
{
    private readonly ILogger<BankRepository> _logger;

    public BankRepository(ApplicationDbContext dbContext, ILogger<BankRepository> logger)
        : base(dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Bank> GetOrCreateAsync(string bankId, string bankName, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(bankId, nameof(bankId));
        ArgumentNullException.ThrowIfNull(bankName, nameof(bankName));
        
        var bank = await DbSet
            .FirstOrDefaultAsync(b => b.BankId == bankId, ct);
        
        if (bank != null)
        {
            return bank;
        }
        
        _logger.LogInformation("Создание нового банка: {BankId}, {BankName}", bankId, bankName);
        
        var newBank = new Bank
        {
            BankId = bankId,
            Name = bankName
        };
        
        return await CreateAsync(newBank, ct);
    }
}
