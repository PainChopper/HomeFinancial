namespace HomeFinancial.OfxParser;

/// <summary>
/// Представляет выписку по банковскому счету в формате OFX
/// </summary>
/// <param name="BankId">Идентификатор банка</param>
/// <param name="AccountId">Идентификатор счета</param>
/// <param name="AccountType">Тип счета (например, CHECKING, SAVINGS, CREDITCARD и т.д.)</param>
/// <param name="Transactions">Коллекция транзакций в выписке, предоставленная как асинхронный поток</param>
public record OfxAccountStatementDto(
    string BankId,
    string AccountId,
    string AccountType,
    IAsyncEnumerable<OfxTransactionDto> Transactions
);