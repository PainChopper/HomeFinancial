using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Категория финансовой транзакции
/// </summary>
public class Category : Entity
{
    private readonly List<BankTransaction> _transactions = new();

    /// <summary>
    /// Название категории
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Транзакции, относящиеся к данной категории
    /// </summary>
    public IReadOnlyCollection<BankTransaction> Transactions => _transactions.AsReadOnly();

    // Конструктор для EF Core
    protected Category()
    {
    }

    /// <summary>
    /// Создает новую категорию с указанным названием
    /// </summary>
    /// <param name="name">Название категории</param>
    public Category(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название категории не может быть пустым", nameof(name));

        Name = name;
    }

    /// <summary>
    /// Изменяет название категории
    /// </summary>
    /// <param name="name">Новое название категории</param>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название категории не может быть пустым", nameof(name));

        Name = name;
    }

    /// <summary>
    /// Добавляет транзакцию в категорию
    /// </summary>
    /// <param name="transaction">Транзакция для добавления</param>
    public void AddTransaction(BankTransaction transaction)
    {
        _transactions.Add(transaction);
    }
}
