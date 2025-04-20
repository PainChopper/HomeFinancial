using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Импортированный файл с банковскими транзакциями
/// </summary>
public class ImportedFile : Entity
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public int Id { get; set; }

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

    // Публичный конструктор для EF Core и маппинга
    public ImportedFile() { }

    // Конструктор для создания новой записи об импортированном файле
    public ImportedFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Имя файла не может быть пустым", nameof(fileName));

        FileName = fileName;
        ImportedAt = DateTime.UtcNow;
    }

    // Публичный конструктор с параметрами для поддержки маппинга Mapperly
    public ImportedFile(int id, string fileName, DateTime importedAt)
    {
        Id = id;
        FileName = fileName;
        ImportedAt = importedAt;
    }
}
