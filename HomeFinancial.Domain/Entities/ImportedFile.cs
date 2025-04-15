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
    public string FileName { get; private set; } = null!;

    /// <summary>
    /// Дата и время импорта файла
    /// </summary>
    public DateTime ImportedAt { get; private set; }

    // Конструктор для EF Core
    protected ImportedFile()
    {
    }

    /// <summary>
    /// Создает новую запись об импортированном файле
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    public ImportedFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Имя файла не может быть пустым", nameof(fileName));

        FileName = fileName;
        ImportedAt = DateTime.UtcNow;
    }
}
