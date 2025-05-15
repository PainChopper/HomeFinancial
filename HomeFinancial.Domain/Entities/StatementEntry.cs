using HomeFinancial.Domain.Common;
using JetBrains.Annotations;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Банковская транзакция
/// </summary>
public class StatementEntry : Entity
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
    /// Предполагается, что категория будет изменяться после импорта
    /// </summary>
    public required int CategoryId { get; init; }

    /// <summary>
    /// Категория транзакции
    /// </summary>
    public EntryCategory? Category { get; [UsedImplicitly] set; }

    /// <summary>
    /// Идентификатор импортированного файла
    /// </summary>
    public required int FileId { get; init; }

    /// <summary>
    /// Импортированный файл
    /// </summary>
    public BankFile? File { get; [UsedImplicitly] set; }

    /// <summary>
    /// Внешний ключ на банковский счет
    /// </summary>
    public required int BankAccountId { get; init; }

    /// <summary>
    /// Банковский счет, к которому относится запись
    /// </summary>
    public BankAccount? BankAccount { get; [UsedImplicitly] set; }
}
