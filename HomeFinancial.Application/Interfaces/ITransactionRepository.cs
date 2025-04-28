using HomeFinancial.Application.Dtos;

namespace HomeFinancial.Application.Interfaces;

public interface ITransactionRepository
{
    /// <summary>
    /// Выполняет пакетную вставку банковских транзакций в базу данных с использованием команды COPY PostgreSQL.
    /// </summary>
    /// <param name="transactions">Список банковских транзакций для вставки.</param>
    /// <param name="cancellationToken">Токен отмены для прерывания операции.</param>
    /// <returns>Кортеж: (количество созданных транзакций, количество пропущенных дубликатов)</returns>
    Task<(int InsertedCount, int SkippedDuplicateCount)> BulkInsertCopyAsync(
        IList<TransactionInsertDto> transactions, 
        CancellationToken cancellationToken = default);
}
