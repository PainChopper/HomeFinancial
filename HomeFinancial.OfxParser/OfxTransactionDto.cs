namespace HomeFinancial.OfxParser;

public record OfxTransactionDto(
    string? TranId,
    DateTime? TranDate,
    string? Category,
    string? Description,
    decimal? Amount
);