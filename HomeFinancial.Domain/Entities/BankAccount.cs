using HomeFinancial.Domain.Common;
using JetBrains.Annotations;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Банковский счет клиента
/// </summary>
public class BankAccount : Entity
{
    /// <summary>
    /// Номер счета (ACCTID)
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Тип счета (ACCTTYPE)
    /// </summary>
    public required string AccountType { get; init; }
    
    /// <summary>
    /// Идентификатор банка (внешний ключ)
    /// </summary>
    public required int BankId { get; init; }

    /// <summary>
    /// Банк, к которому относится счет
    /// </summary>
    public Bank? Bank { get; [UsedImplicitly] set; }
}
