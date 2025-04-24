using JetBrains.Annotations;
using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Банковская транзакция
/// </summary>
public record BankTransaction : Entity
{
    /// <summary>
    /// Уникальный идентификатор транзакции в банковской системе (Financial Institution Transaction ID)
    /// </summary>
    public required string FitId { get; init; }

    /// <summary>
    /// Дата транзакции
    /// </summary>
    public required DateTime Date { get; init; }

    /// <summary>
    /// Сумма транзакции (положительная для поступлений, отрицательная для расходов)
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Описание транзакции
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public int? CategoryId { get; [UsedImplicitly] set; }

    /// <summary>
    /// Категория транзакции
    /// </summary>
    public required Category Category { get; init; }

    /// <summary>
    /// Идентификатор импортированного файла
    /// </summary>
    public int? ImportedFileId { get; [UsedImplicitly] set; }

    /// <summary>
    /// Импортированный файл
    /// </summary>
    public required ImportedFile ImportedFile { get; init; }
}
