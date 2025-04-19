using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Категория финансовой транзакции
/// </summary>
public class Category : Entity
{
    /// <summary>
    /// Название категории
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Транзакции, относящиеся к данной категории
    /// </summary>
    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();

    // Публичный конструктор для EF Core и маппинга
    public Category() { }

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
        Transactions.Add(transaction);
    }

    /// <summary>
    /// Создает новую категорию с указанным идентификатором и названием
    /// </summary>
    /// <param name="id">Идентификатор категории</param>
    /// <param name="name">Название категории</param>
    public Category(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
