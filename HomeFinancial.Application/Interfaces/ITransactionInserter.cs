using HomeFinancial.Application.Dtos;
using HomeFinancial.Application.UseCases.ImportOfxFile;

namespace HomeFinancial.Application.Interfaces;

public interface ITransactionInserter
{
    /// <summary>
    /// Выполняет пакетную вставку банковских транзакций в базу данных с использованием команды COPY PostgreSQL.
    /// </summary>
    /// <param name="transactions">Список банковских транзакций для вставки.</param>
    /// <param name="ct">Токен отмены для прерывания операции.</param>
    /// <returns>Результат операции вставки: количество созданных транзакций и количество пропущенных дубликатов</returns>
    Task<BulkInsertResult> BulkInsertCopyAsync(
        IList<TransactionInsertDto> transactions, 
        CancellationToken ct = default);
}
