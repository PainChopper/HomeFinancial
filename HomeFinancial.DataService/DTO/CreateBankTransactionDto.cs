namespace HomeFinancial.DataService.DTO;

public class CreateBankTransactionDto
{
    public string FitId { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
}