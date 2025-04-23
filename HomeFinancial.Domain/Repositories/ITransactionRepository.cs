using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с банковскими транзакциями
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Создает диапазон транзакций
    /// </summary>
    /// <param name="transactions">Транзакции для создания</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Количество успешно сохраненных транзакций</returns>
    Task<int> CreateRangeAsync(IList<BankTransaction> transactions, CancellationToken cancellationToken = default);
    
    Task<int> BulkInsertCopyAsync(IList<BankTransaction> transactions, CancellationToken cancellationToken = default);
}
