using HomeFinancial.Domain.Common;
using JetBrains.Annotations;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Импортированный файл с банковскими транзакциями
/// </summary>
public class BankFile : Entity
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
    public required BankFileStatus Status { get; [UsedImplicitly] set; }
}
