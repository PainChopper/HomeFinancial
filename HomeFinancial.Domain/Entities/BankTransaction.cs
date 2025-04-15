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
    public string FitId { get; private set; } = null!;

    /// <summary>
    /// Дата транзакции
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    /// Сумма транзакции (положительная для поступлений, отрицательная для расходов)
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Описание транзакции
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public int CategoryId { get; private set; }

    /// <summary>
    /// Категория транзакции
    /// </summary>
    public Category? Category { get; private set; }

    // Конструктор для EF Core
    protected BankTransaction()
    {
    }

    /// <summary>
    /// Создает новую банковскую транзакцию
    /// </summary>
    /// <param name="fitId">Уникальный идентификатор транзакции в банковской системе</param>
    /// <param name="date">Дата транзакции</param>
    /// <param name="amount">Сумма транзакции</param>
    /// <param name="description">Описание транзакции</param>
    /// <param name="categoryId">Идентификатор категории</param>
    public BankTransaction(string fitId, DateTime date, decimal amount, string description, int categoryId)
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
    }

    /// <summary>
    /// Изменяет категорию транзакции
    /// </summary>
    /// <param name="category">Новая категория</param>
    public void UpdateCategory(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        Category = category;
        CategoryId = category.Id;
    }

    /// <summary>
    /// Обновляет описание транзакции
    /// </summary>
    /// <param name="description">Новое описание</param>
    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Описание не может быть пустым", nameof(description));

        Description = description;
    }
}
