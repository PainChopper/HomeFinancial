namespace HomeFinancial.Data.Models;

public class BankTransaction
{
    public int Id { get; set; }
    public string FitId { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;

    // Связь с категорией
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}

public class Category
{
    public int Id { get; set; }

    // Храним в БД с заглавной буквой (см. логику "приведения" в DataService или др.)
    public string Name { get; set; } = null!;

    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
}

public class ImportedFile
{
    public int Id { get; set; }
    public string FileName { get; set; } = null!; // Уникальное имя файла (например, "operations 2025-01-18.ofx")
    public DateTime ImportedAt { get; set; }
} 