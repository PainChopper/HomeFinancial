using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Банковский счет клиента
/// </summary>
public class BankAccount : Entity
{
    /// <summary>
    /// Идентификатор банка (внешний ключ)
    /// </summary>
    public int BankId { get; set; }

    /// <summary>
    /// Банк, к которому относится счет
    /// </summary>
    public Bank Bank { get; set; } = null!;

    /// <summary>
    /// Номер счета (ACCTID)
    /// </summary>
    public string AccountId { get; set; } = null!;

    /// <summary>
    /// Тип счета (ACCTTYPE)
    /// </summary>
    public string AccountType { get; set; } = null!;
}
