using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Импортированный файл с банковскими транзакциями
/// </summary>
public class ImportedFile : Entity
{
    /// <summary>
    /// Имя файла
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// Дата и время импорта файла
    /// </summary>
    public DateTime ImportedAt { get; set; }

    /// <summary>
    /// Статус файла
    /// </summary>
    public ImportedFileStatus Status { get; set; }

    /// <summary>
    /// JSON сериализованный результат импорта (ImportResult).
    /// </summary>
    public string? ImportResultJson { get; set; }

    // Публичный конструктор для EF Core и маппинга
    public ImportedFile() { }

    // Конструктор для создания новой записи об импортированном файле
    public ImportedFile(string fileName)
    {
        FileName = fileName;
    }
}
