using HomeFinancial.Domain.Common;
using HomeFinancial.Domain.Services;

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

    /// <summary>
    /// JSON сериализованный результат импорта (ImportResult).
    /// </summary>
    public string? ImportResultJson { get; set; }

    /// <summary>
    /// Внешний ключ пользователя (Identity).
    /// </summary>
    public long UserId { get; set; }


    // Публичный конструктор для EF Core и маппинга
    public ImportedFile() { }

    // Конструктор для создания новой записи об импортированном файле
    public ImportedFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Имя файла не может быть пустым", nameof(fileName));

        FileName = fileName;
    }

    // Публичный конструктор с параметрами для поддержки маппинга Mapperly
    public ImportedFile(int id, string fileName, DateTime importedAt)
    {
        Id = id;
        FileName = fileName;
        ImportedAt = importedAt;
    }

    // Удалить прямое использование DateTime.UtcNow, использовать через сервис в вызывающем коде
    public void SetImportedAt(IDateTimeProvider provider) => ImportedAt = provider.UtcNow;
}
