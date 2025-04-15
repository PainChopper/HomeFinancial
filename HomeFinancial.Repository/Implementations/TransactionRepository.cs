using HomeFinancial.Data;
using HomeFinancial.Data.Models;
using HomeFinancial.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Repository.Implementations;

/// <summary>
/// Репозиторий для работы с банковскими транзакциями
/// </summary>
public class TransactionRepository : GenericRepository<BankTransaction>, ITransactionRepository
{
    public TransactionRepository(HomeFinancialDbContext dbContext, ILogger<GenericRepository<BankTransaction>> logger)
        : base(dbContext, logger)
    {
    }

    /// <summary>
    /// Создает новую банковскую транзакцию
    /// </summary>
    /// <param name="transaction">Транзакция для создания</param>
    /// <returns>Созданная транзакция</returns>
    /// <exception cref="InvalidOperationException">Если транзакция с таким FITID уже существует</exception>
    public override async Task<BankTransaction> CreateAsync(BankTransaction transaction)
    {
        Logger.LogInformation("Attempting to add transaction with FITID: {FitId}", transaction.FitId);
        if (!await DbSet.AnyAsync(x => x.FitId == transaction.FitId))
        {
            return await base.CreateAsync(transaction);
        }
        Logger.LogWarning("Transaction with FITID={FitId} already exists.", transaction.FitId);
        throw new InvalidOperationException($"FITID={transaction.FitId} already exists.");
    }
}
