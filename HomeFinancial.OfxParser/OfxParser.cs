using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.OfxParser;

/// <summary>
/// Реализация парсера OFX-файлов
/// </summary>
public class OfxParser : IOfxParser
{
    private readonly ILogger<OfxParser> _logger;

    public OfxParser(ILogger<OfxParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Читает OFX-файл из потока и возвращает перечисление транзакций.
    /// </summary>
    /// <param name="stream">Поток OFX-файла</param>
    /// <returns>Перечисление транзакций</returns>
    public IEnumerable<OfxTransaction> ParseOfxFile(Stream stream)
    {
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreWhitespace = true });

        // Переходим к корневому элементу
        while (reader.Read() && reader.NodeType != XmlNodeType.Element) { }

        // Ищем элементы STMTTRN
        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "STMTTRN")
            {
                continue;
            }

            if (XNode.ReadFrom(reader) is not XElement trnElement)
            {
                continue;
            }

            var fitIdValue = trnElement.Element("FITID")?.Value;
            var dtPostedRaw = trnElement.Element("DTPOSTED")?.Value;
            var memoValue = trnElement.Element("MEMO")?.Value;
            var nameValue = trnElement.Element("NAME")?.Value;
            var trnAmtValue = trnElement.Element("TRNAMT")?.Value;

            var parsedDate = TryParseOfxDate(dtPostedRaw, _logger, fitIdValue ?? "");

            decimal parsedAmount = 0;
            if (decimal.TryParse(trnAmtValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt))
            {
                parsedAmount = amt;
            }

            var transaction = new OfxTransaction(
                TranId: fitIdValue,
                TranDate: parsedDate,
                Category: memoValue,
                Description: nameValue,
                Amount: parsedAmount
            );

            yield return transaction;
        }
    }

    // Константа с поддерживаемыми форматами дат OFX (только для чтения)
    private static readonly IReadOnlyList<string> OfxDateFormats 
        = Array.AsReadOnly(["yyyyMMddHHmmss.fff", "yyyyMMddHHmmss"]);

    /// <summary>
    /// Пробует разобрать дату OFX в нескольких форматах.
    /// </summary>
    private static DateTime? TryParseOfxDate(string? dtPostedRaw, ILogger logger, string? fitId)
    {
        if (string.IsNullOrWhiteSpace(dtPostedRaw))
        {
            logger.LogWarning("Значение DTPOSTED отсутствует или пустое для FITID: {FitId}", fitId);
            return null;
        }

        // Используем константу для форматов
        foreach (var format in OfxDateFormats)
        {
            var len = format.Length;
            if (dtPostedRaw.Length < len)
            {
                continue;
            }

            var candidate = dtPostedRaw.Substring(0, len);
            if (!DateTime.TryParseExact(candidate, format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var parsed))
            {
                continue;
            }
            
            logger.LogDebug("Дата разобрана: {ParsedDate} (формат {Format})", parsed, format);
            return parsed;
        }
        logger.LogWarning("Не удалось разобрать дату '{DtPostedRaw}' для FITID: {FitId}", dtPostedRaw, fitId);
        return null;
    }
}