using HomeFinancial.Application.Dtos;
using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Контекст импорта OFX-файла
/// </summary>
internal sealed class ImportContext
{
    /// <summary>
    /// Пакет DTO для пакетной вставки
    /// </summary>
    public List<TransactionInsertDto> Batch { get; }
    
    /// <summary>
    /// Метрики импортирования
    /// </summary>
    public StatementProcessMetrics Metrics { get; set; }
    
    /// <summary>
    /// Импортируемый файл
    /// </summary>
    public BankFile ImportedFile { get; }
    
    /// <summary>
    /// Идентификатор сессии импорта
    /// </summary>
    public ImportFileSession Session { get; }

    /// <summary>
    /// Создает новый экземпляр контекста импорта
    /// </summary>
    /// <param name="session">Сессия импорта</param>
    /// <param name="batchSize">Размер пакета для вставки</param>
    public ImportContext(ImportFileSession session, int batchSize)
    {
        Session = session;
        ImportedFile = session.File;
        Batch = new List<TransactionInsertDto>(batchSize);
    }
}
