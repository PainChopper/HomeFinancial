using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Банковская транзакция
/// </summary>
public class BankTransaction : Entity
{
    /// <summary>
    /// Уникальный идентификатор транзакции в банковской системе (Financial Institution Transaction ID)
    /// </summary>
    public string FitId { get; set; } = null!;

    /// <summary>
    /// Дата транзакции
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Сумма транзакции (положительная для поступлений, отрицательная для расходов)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Описание транзакции
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Категория транзакции
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Идентификатор импортированного файла
    /// </summary>
    public int ImportedFileId { get; set; }

    /// <summary>
    /// Импортированный файл
    /// </summary>
    public ImportedFile? ImportedFile { get; set; }

    // Публичный конструктор для EF Core и маппинга
    public BankTransaction() { }

    /// <summary>
    /// Создает новую банковскую транзакцию
    /// </summary>
    /// <param name="fitId">Уникальный идентификатор транзакции в банковской системе</param>
    /// <param name="date">Дата транзакции</param>
    /// <param name="amount">Сумма транзакции</param>
    /// <param name="description">Описание транзакции</param>
    /// <param name="categoryId">Идентификатор категории</param>
    /// <param name="importedFileId">Идентификатор импортированного файла</param>
    public BankTransaction(string fitId, DateTime date, decimal amount, string description, int categoryId, int importedFileId)
    {
        if (string.IsNullOrWhiteSpace(fitId))
            throw new ArgumentException("FITID не может быть пустым", nameof(fitId));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Описание не может быть пустым", nameof(description));

        FitId = fitId;
        Date = date;
        Amount = amount;
        Description = description;
        CategoryId = categoryId;
        ImportedFileId = importedFileId;
    }

    /// <summary>
    /// Публичный конструктор с параметрами для поддержки маппинга Mapperly
    /// </summary>
    /// <param name="id">Идентификатор транзакции</param>
    /// <param name="fitId">Уникальный идентификатор транзакции в банковской системе</param>
    /// <param name="date">Дата транзакции</param>
    /// <param name="amount">Сумма транзакции</param>
    /// <param name="description">Описание транзакции</param>
    /// <param name="categoryId">Идентификатор категории</param>
    /// <param name="importedFileId">Идентификатор импортированного файла</param>
    public BankTransaction(int id, string fitId, DateTime date, decimal amount, string description, int categoryId, int importedFileId)
    {
        Id = id;
        FitId = fitId;
        Date = date;
        Amount = amount;
        Description = description;
        CategoryId = categoryId;
        ImportedFileId = importedFileId;
    }
}
