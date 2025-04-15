using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с банковскими транзакциями
/// </summary>
public interface IBankTransactionRepository : IRepository<BankTransaction>
{
    /// <summary>
    /// Проверяет, существует ли транзакция с указанным FITID
    /// </summary>
    /// <param name="fitId">FITID транзакции</param>
    /// <returns>true, если транзакция существует; иначе false</returns>
    Task<bool> ExistsByFitIdAsync(string fitId);
    
    /// <summary>
    /// Получает транзакцию по FITID
    /// </summary>
    /// <param name="fitId">FITID транзакции</param>
    /// <returns>Транзакция или null, если транзакция не найдена</returns>
    Task<BankTransaction?> GetByFitIdAsync(string fitId);
    
    /// <summary>
    /// Получает транзакции по категории
    /// </summary>
    /// <param name="categoryId">Идентификатор категории</param>
    /// <returns>Список транзакций</returns>
    Task<List<BankTransaction>> GetByCategoryIdAsync(int categoryId);
}
