namespace HomeFinancial.Application.Dtos;

/// <summary>
/// DTO для вставки транзакции
/// </summary>
public record TransactionInsertDto(
    int FileId,
    string FitId,
    DateTime Date,
    decimal Amount,
    string Description,
    int CategoryId
);
