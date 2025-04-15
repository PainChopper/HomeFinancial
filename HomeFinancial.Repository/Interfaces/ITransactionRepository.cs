using HomeFinancial.Data.Models;

namespace HomeFinancial.Repository.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с банковскими транзакциями
/// </summary>
public interface ITransactionRepository : IGenericRepository<BankTransaction>
{
    // Здесь можно добавить специфичные методы для транзакций
}
