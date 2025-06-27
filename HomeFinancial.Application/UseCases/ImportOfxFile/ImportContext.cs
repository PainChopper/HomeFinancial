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
    /// Количество вставленных транзакций
    /// </summary>
    public int Inserted { get; set; }
    
    /// <summary>
    /// Количество пропущенных дубликатов
    /// </summary>
    public int Duplicates { get; set; }
    
    /// <summary>
    /// Количество ошибок
    /// </summary>
    public int Errors { get; set; }
    
    /// <summary>
    /// Общее количество обработанных транзакций
    /// </summary>
    public int Total => Inserted + Duplicates + Errors;
    
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
