namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Базовый DTO для банковской транзакции из OFX-файла
/// </summary>
public abstract class BaseTransactionDto
{
    /// <summary>
    /// Идентификатор транзакции
    /// </summary>
    public string? TranId { get; set; }

    /// <summary>
    /// Дата транзакции
    /// </summary>
    public DateTime? TranDate { get; set; }

    /// <summary>
    /// Категория
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Сумма транзакции
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Идентификатор файла-источника (FK)
    /// </summary>
    public int FileId { get; set; }
}
