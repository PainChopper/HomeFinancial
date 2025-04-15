namespace HomeFinancial.DataService.DTO;

public class BankTransactionDto
{
    public int Id { get; set; }
    public string FitId { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public int CategoryId { get; set; }
}