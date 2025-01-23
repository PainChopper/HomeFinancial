// DTOs.cs
namespace HomeFinancial.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = null!;
    }

    public class BankTransactionDto
    {
        public int Id { get; set; }
        public string FitId { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public int CategoryId { get; set; }
    }

    public class CreateBankTransactionDto
    {
        public string FitId { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
    }

    public class ImportedFileDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public DateTime ImportedAt { get; set; }
    }

    public class CreateImportedFileDto
    {
        public string FileName { get; set; } = null!;
    }
}