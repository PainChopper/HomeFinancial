namespace HomeFinancial.Repository.Models;

public class Category
{
    public int Id { get; set; }

    // Храним в БД с заглавной буквой (см. логику "приведения" в DataService или др.)
    public string Name { get; set; } = null!;

    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
}