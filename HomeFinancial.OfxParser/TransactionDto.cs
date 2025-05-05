namespace HomeFinancial.OfxParser;

public record TransactionDto(
    string TranId,
    DateTime TranDate,
    string Category,
    string Description,
    decimal Amount
);