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