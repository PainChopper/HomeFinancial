using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Application.Common;

/// <summary>
/// Реализация Unit of Work
/// </summary>
public class ImportBatchFilesUnitOfWork : IUnitOfWork
{
    public ImportBatchFilesUnitOfWork(IRepository<BankTransaction> transactions, IRepository<ImportedFile> importedFiles)
    {
        Transactions = transactions;
        ImportedFiles = importedFiles;
    }

    /// <summary>
    /// Репозиторий банковских транзакций
    /// </summary>
    public IRepository<BankTransaction> Transactions { get; }
    
    /// <summary>
    /// Репозиторий импортированных файлов
    /// </summary>
    public IRepository<ImportedFile> ImportedFiles { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Освобождает ресурсы
    /// </summary>
    void Dispose()
    {
    
    }

    void IDisposable.Dispose()
    {
        Dispose();
    }
}
