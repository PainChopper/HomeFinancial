namespace HomeFinancial.OfxParser;

public record OfxTransaction(
    string TranId,
    DateTime TranDate,
    string Category,
    string Description,
    decimal Amount
);