namespace HomeFinancial.OfxParser;

/// <summary>
/// Интерфейс парсера OFX-файлов
/// </summary>
public interface IOfxParser
{
    /// <summary>
    /// Читает OFX-файл из потока и возвращает перечисление транзакций.
    /// </summary>
    /// <param name="stream">Поток OFX-файла</param>
    /// <returns>Перечисление транзакций</returns>
    IEnumerable<OfxTransaction> ParseOfxFile(Stream stream);
}