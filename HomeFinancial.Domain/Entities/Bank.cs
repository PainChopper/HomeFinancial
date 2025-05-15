using System.Diagnostics.CodeAnalysis;
using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Банк, участвующий в операциях
/// </summary>
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class Bank  : Entity
{
    /// <summary>
    /// Краткое наименование из файла OFX
    /// </summary>
    public required string BankId { get; init; }

    /// <summary>
    /// Полное название банка
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// БИК (банковский идентификационный код)
    /// </summary>
    public string? Bic { get; set; }

    /// <summary>
    /// SWIFT/BIC-код
    /// </summary>
    public string? Swift { get; set; }

    /// <summary>
    /// ИНН
    /// </summary>
    public string? Inn { get; set; }

    /// <summary>
    /// КПП
    /// </summary>
    public string? Kpp { get; set; }

    /// <summary>
    /// Регистрационный номер (лицензия ЦБ)
    /// </summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Адрес банка
    /// </summary>
    public string? Address { get; set; }
}
