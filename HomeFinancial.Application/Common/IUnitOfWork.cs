using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Application.Common;

/// <summary>
/// Единица работы (Unit of Work) для управления транзакциями
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Репозиторий банковских транзакций
    /// </summary>
    IRepository<BankTransaction> Transactions { get; }
    
    /// <summary>
    /// Репозиторий импортированных файлов
    /// </summary>
    IRepository<ImportedFile> ImportedFiles { get; }
    
    /// <summary>
    /// Сохраняет все изменения в базу данных
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Количество затронутых записей</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
