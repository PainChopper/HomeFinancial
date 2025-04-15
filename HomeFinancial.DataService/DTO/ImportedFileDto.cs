namespace HomeFinancial.DataService.DTO;

public class ImportedFileDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = null!;
    public DateTime ImportedAt { get; set; }
}