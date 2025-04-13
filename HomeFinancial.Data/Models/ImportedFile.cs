namespace HomeFinancial.Data.Models;

public class ImportedFile
{
    public int Id { get; set; }
    public string FileName { get; set; } = null!; // Уникальное имя файла (например, "operations 2025-01-18.ofx")
    public DateTime ImportedAt { get; set; }
}