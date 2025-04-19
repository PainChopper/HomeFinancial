namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Команда для импорта OFX-файла
/// </summary>
public record ImportOfxFileCommand(
    /// <summary>
    /// Имя файла
    /// </summary>
    string FileName, 
    /// <summary>
    /// Поток файла
    /// </summary>
    Stream FileStream);
