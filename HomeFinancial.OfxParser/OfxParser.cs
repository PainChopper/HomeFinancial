using System.Globalization;
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
    /// Читает OFX-файл из потока и возвращает список транзакций.
    /// </summary>
    /// <param name="stream">Поток OFX-файла</param>
    /// <returns>Список транзакций</returns>
    public List<OfxTransaction> ParseOfxFile(Stream stream)
    {
        _logger.LogInformation("Начало разбора OFX файла из потока.");

        // Загрузка документа
        // Важно: если OFX содержит отдельные куски "<?xml?>", это иногда может нарушить работу XDocument.
        // Обычно достаточно загрузить файл напрямую (XDocument.Load).
        // Если есть проблемы с заголовками, их можно удалить вручную заранее.
        XDocument xdoc;
        try
        {
            xdoc = XDocument.Load(stream);
            _logger.LogInformation("Документ OFX успешно загружен.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке OFX документа: {ErrorMessage}", ex.Message);
            throw;
        }

        // Получаем все элементы транзакций
        var stmtTrnElements = xdoc.Descendants("STMTTRN").ToList();
        _logger.LogInformation("Найдено элементов транзакций: {Count}.", stmtTrnElements.Count);

        var transactions = new List<OfxTransaction>();

        foreach (var element in stmtTrnElements)
        {
            var fitIdValue = element.Element("FITID")?.Value;
            var dtPostedRaw = element.Element("DTPOSTED")?.Value;
            var memoValue = element.Element("MEMO")?.Value;
            var nameValue = element.Element("NAME")?.Value;
            var trnAmtValue = element.Element("TRNAMT")?.Value;

            _logger.LogDebug("Обрабатывается транзакция FITID: {FitId}", fitIdValue);

            var parsedDate = TryParseOfxDate(dtPostedRaw, _logger, fitIdValue ?? "");

            // Разбор суммы
            decimal parsedAmount = 0;
            if (decimal.TryParse(trnAmtValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt))
            {
                parsedAmount = amt;
                _logger.LogDebug("Сумма разобрана: {Amount}", parsedAmount);
            }
            else
            {
                _logger.LogWarning("Не удалось разобрать сумму: {TrnAmtValue} для FITID: {FitId}", trnAmtValue, fitIdValue);
            }

            // Создаём DTO и добавляем в список
            var transaction = new OfxTransaction(
                TranId: fitIdValue,
                TranDate: parsedDate,
                Category: memoValue,
                Description: nameValue,
                Amount: parsedAmount
            ); 

            transactions.Add(transaction);
            _logger.LogDebug("Добавлена транзакция с ID: {TranId}", transaction.TranId);
        }

        _logger.LogInformation("Разбор OFX файла завершен. Всего транзакций обработано: {Count}.", transactions.Count);
        return transactions;
    }

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

        string[] formats = { "yyyyMMddHHmmss.fff", "yyyyMMddHHmmss" };
        foreach (var format in formats)
        {
            int len = format.Length;
            if (dtPostedRaw.Length >= len)
            {
                string candidate = dtPostedRaw.Substring(0, len);
                if (DateTime.TryParseExact(candidate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    logger.LogDebug("Дата разобрана: {ParsedDate} (формат {Format})", parsed, format);
                    return parsed;
                }
            }
        }
        logger.LogWarning("Не удалось разобрать дату '{DtPostedRaw}' для FITID: {FitId}", dtPostedRaw, fitId);
        return null;
    }
}