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

    /// <summary>
    /// Создаёт экземпляр <see cref="OfxParser"/>.
    /// </summary>
    /// <param name="logger">Экземпляр логгера.</param>
    public OfxParser(ILogger<OfxParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Читает OFX-файл и возвращает список транзакций.
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Список транзакций</returns>
    public List<OfxTransaction> ParseOfxFile(string filePath)
    {
        _logger.LogInformation("Начало разбора OFX файла: {FilePath}", filePath);

        // Загрузка документа
        // Важно: если OFX содержит отдельные куски "<?xml?>", это иногда может нарушить работу XDocument.
        // Обычно достаточно загрузить файл напрямую (XDocument.Load).
        // Если есть проблемы с заголовками, их можно удалить вручную заранее.
        XDocument xdoc;
        try
        {
            xdoc = XDocument.Load(filePath);
            _logger.LogInformation("Документ OFX успешно загружен.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке OFX документа: {ErrorMessage}", ex.Message);
            throw;
        }

        // Получаем все элементы транзакций
        var stmtTrnElements = xdoc.Descendants("STMTTRN");
        int transactionCount = stmtTrnElements.Count();
        _logger.LogInformation("Найдено элементов транзакций: {Count}.", transactionCount);

        var transactions = new List<OfxTransaction>();

        foreach (var element in stmtTrnElements)
        {
            var fitIdValue = element.Element("FITID")?.Value;
            var dtPostedRaw = element.Element("DTPOSTED")?.Value;
            var memoValue = element.Element("MEMO")?.Value;
            var nameValue = element.Element("NAME")?.Value;
            var trnAmtValue = element.Element("TRNAMT")?.Value;

            _logger.LogDebug("Обрабатывается транзакция FITID: {FitId}", fitIdValue);

            // Разбор даты
            // Формат примерно "20250113224820.000[+3:MSK]".
            DateTime parsedDate = default;
            if (!string.IsNullOrWhiteSpace(dtPostedRaw))
            {
                try
                {
                    parsedDate = DateTime.ParseExact(
                        dtPostedRaw.Substring(0, 17),
                        "yyyyMMddHHmmss.fff",
                        CultureInfo.InvariantCulture
                    );
                    _logger.LogDebug("Дата с миллисекундами разобрана: {ParsedDate}", parsedDate);
                }
                catch (FormatException)
                {
                    try
                    {
                        parsedDate = DateTime.ParseExact(
                            dtPostedRaw.Substring(0, 14),
                            "yyyyMMddHHmmss",
                            CultureInfo.InvariantCulture
                        );
                        _logger.LogDebug("Дата без миллисекунд разобрана: {ParsedDate}", parsedDate);
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogWarning("Не удалось разобрать дату '{DtPostedRaw}': {ErrorMessage}", dtPostedRaw, ex.Message);
                        // Можно продолжить или выбросить исключение
                    }
                }
            }
            else
            {
                _logger.LogWarning("Значение DTPOSTED отсутствует или пустое для FITID: {FitId}", fitIdValue);
            }

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
                fitIdValue ?? string.Empty,
                parsedDate,
                memoValue ?? string.Empty,
                nameValue ?? string.Empty,
                parsedAmount
            );

            transactions.Add(transaction);
            _logger.LogDebug("Добавлена транзакция с ID: {TranId}", transaction.TranId);
        }

        _logger.LogInformation("Разбор OFX файла завершен. Всего транзакций обработано: {Count}.", transactions.Count);
        return transactions;
    }
}