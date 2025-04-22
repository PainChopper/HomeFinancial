using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Унифицированный интерфейс репозитория для банковских транзакций (базовый)
/// </summary>
public interface IBankTransactionRepository : IGenericRepository<BankTransaction>
{
    Task<bool> ExistsByFitIdAsync(string fitId);
    Task<BankTransaction?> GetByFitIdAsync(string fitId);
    Task<List<BankTransaction>> GetByCategoryIdAsync(int categoryId);
}
