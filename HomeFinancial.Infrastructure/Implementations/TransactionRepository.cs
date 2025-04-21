using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Infrastructure.Implementations;

/// <summary>
/// Репозиторий для работы с банковскими транзакциями
/// </summary>
public class TransactionRepository(
    ApplicationDbContext dbContext,
    ILogger<GenericGenericRepository<BankTransaction>> logger)
    : GenericGenericRepository<BankTransaction>(dbContext, logger), ITransactionRepository
{
    /// <summary>
    /// Создает новую банковскую транзакцию
    /// </summary>
    public override async Task<BankTransaction> CreateAsync(BankTransaction transaction, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Attempting to add transaction with FITID: {FitId}", transaction.FitId);
        if (!await ExistsByFitIdAsync(transaction.FitId))
            return await base.CreateAsync(transaction, cancellationToken);
        Logger.LogWarning("Transaction with FITID={FitId} already exists.", transaction.FitId);
        throw new InvalidOperationException($"FITID={transaction.FitId} already exists.");
    }

    /// <summary>
    /// Проверяет, существует ли транзакция с указанным FITID
    /// </summary>
    public async Task<bool> ExistsByFitIdAsync(string fitId)
        => await DbSet.AnyAsync(x => x.FitId == fitId);

    /// <summary>
    /// Получает транзакцию по FITID
    /// </summary>
    public async Task<BankTransaction?> GetByFitIdAsync(string fitId)
        => await DbSet.FirstOrDefaultAsync(x => x.FitId == fitId);

    /// <summary>
    /// Получает транзакции по категории
    /// </summary>
    public async Task<List<BankTransaction>> GetByCategoryIdAsync(int categoryId)
        => await DbSet.Where(x => x.CategoryId == categoryId).ToListAsync();
}
