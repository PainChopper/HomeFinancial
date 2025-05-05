namespace HomeFinancial.OfxParser;

/// <summary>
/// Информация о банке и счете
/// </summary>
/// <param name="BankId">Идентификатор банка</param>
/// <param name="AccountId">Идентификатор счета</param>
public record BankAccountDto(
    string BankId,
    string AccountId
);