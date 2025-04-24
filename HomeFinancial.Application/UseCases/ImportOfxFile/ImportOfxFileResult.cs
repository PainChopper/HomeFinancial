namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Результат импорта OFX-файла
/// </summary>
public class ImportOfxFileResult
{
    /// <summary>
    /// Общее количество транзакций в файле
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Количество успешно импортированных транзакций
    /// </summary>
    public int ImportedCount { get; init; }

    /// <summary>
    /// Количество пропущенных дубликатов
    /// </summary>
    public int SkippedDuplicateCount { get; init; }

    /// <summary>
    /// Количество транзакций с ошибками
    /// </summary>
    public int ErrorCount { get; init; }
}