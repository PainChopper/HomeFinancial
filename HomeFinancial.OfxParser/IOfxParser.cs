namespace HomeFinancial.OfxParser;

/// <summary>
/// Интерфейс парсера OFX-файлов
/// </summary>
public interface IOfxParser
{
    /// <summary>
    /// Читает OFX-файл и возвращает список транзакций.
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Список транзакций</returns>
    List<OfxTransaction> ParseOfxFile(string filePath);
}