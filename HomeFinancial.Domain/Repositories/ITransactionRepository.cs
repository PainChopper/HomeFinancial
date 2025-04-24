using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с банковскими транзакциями
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Выполняет пакетную вставку банковских транзакций в базу данных с использованием команды COPY PostgreSQL.
    /// </summary>
    /// <param name="transactions">Список банковских транзакций для вставки.</param>
    /// <param name="cancellationToken">Токен отмены для прерывания операции.</param>
    /// <returns>Количество созданных транзакций</returns>
    Task<TransactionsInsertResult> BulkInsertCopyAsync(IList<BankTransaction> transactions, CancellationToken cancellationToken = default);
}
