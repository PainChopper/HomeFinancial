namespace HomeFinancial.OfxParser;

/// <summary>
/// Интерфейс парсера OFX-файлов
/// </summary>
public interface IOfxParser
{
    /// <summary>
    /// Читает OFX-файл из потока и возвращает список транзакций.
    /// </summary>
    /// <param name="stream">Поток OFX-файла</param>
    /// <returns>Список транзакций</returns>
    List<OfxTransaction> ParseOfxFile(Stream stream);
}