namespace HomeFinancial.OfxParser;

/// <summary>
/// Результат парсинга OFX-файла
/// </summary>
/// <param name="BankAccount">Идентификаторы банка и счета</param>
/// <param name="Transactions">Транзакции из OFX-файла</param>
public record OfxParseResult(
    OfxBankAccountDto BankAccount,
    IAsyncEnumerable<OfxTransactionDto> Transactions
);