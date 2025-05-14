namespace HomeFinancial.OfxParser.Dto;

/// <summary>
/// Представляет информацию о банковской транзакции в формате OFX
/// </summary>
/// <param name="Id">Уникальный идентификатор транзакции</param>
/// <param name="TranType">Тип транзакции (DEBIT, CREDIT и др.)</param>
/// <param name="TranDate">Дата проведения транзакции</param>
/// <param name="Category">Категория транзакции</param>
/// <param name="Description">Описание транзакции</param>
/// <param name="Amount">Сумма транзакции (отрицательная для расходов, положительная для доходов)</param>
public record OfxTransactionDto(
    string Id,
    string TranType,
    DateTime TranDate,
    string Category,
    string Description,
    decimal Amount
);