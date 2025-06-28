using HomeFinancial.Application.Dtos;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Контекст импорта OFX-файла
/// </summary>
public sealed class ImportContext
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
    ///  Идентификатор Импортируемого файла
    /// </summary>
    public int FileId { get; }

    /// <summary>
    /// Создает новый экземпляр контекста импорта
    /// </summary>
    /// <param name="fileId"></param>
    /// <param name="batchSize">Размер пакета для вставки</param>
    public ImportContext(int fileId, int batchSize)
    {
        FileId = fileId;
        Batch = new List<TransactionInsertDto>(batchSize);
    }
}
