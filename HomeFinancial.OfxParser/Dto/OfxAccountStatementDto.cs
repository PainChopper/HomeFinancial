namespace HomeFinancial.OfxParser.Dto;

/// <summary>
/// Представляет выписку по банковскому счету в формате OFX
/// </summary>
/// <param name="BankId">Идентификатор банка</param>
/// <param name="BankAccountId">Идентификатор счета</param>
/// <param name="BankAccountType">Тип счета (например, CHECKING, SAVINGS, CREDITCARD и т.д.)</param>
/// <param name="Transactions">Коллекция транзакций в выписке, предоставленная как асинхронный поток</param>
public record OfxAccountStatementDto(
    string BankId,
    string BankAccountId,
    string BankAccountType,
    IAsyncEnumerable<OfxTransactionDto> Transactions
);