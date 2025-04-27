using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Импортированный файл с банковскими транзакциями
/// </summary>
public record BankFile : Entity
{
    /// <summary>
    /// Имя файла
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Дата и время импорта файла
    /// </summary>
    public required DateTime ImportedAt { get; init; }

    /// <summary>
    /// Статус файла
    /// </summary>
    public required BankFileStatus Status { get; set; }
}
