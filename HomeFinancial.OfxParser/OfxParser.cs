using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.OfxParser;

/// <summary>
/// Реализация парсера OFX-файлов (асинхронная версия)
/// </summary>
public class OfxParser : IOfxParser
{
    private readonly ILogger _logger;

    public OfxParser(ILogger<OfxParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Асинхронно читает OFX-файл из потока и возвращает результат парсинга.
    /// Транзакции читаются лениво (async), без загрузки всего файла в память.
    /// </summary>
    public async Task<OfxParseResult> ParseOfxFileAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreWhitespace = true, Async = true });

        // Переходим к корневому элементу
        while (await reader.ReadAsync() && reader.NodeType != XmlNodeType.Element) 
        {
        }

        var bankAccount = await GetBankAccountInfoAsync(reader);
        var transactions = GetTransactionsAsync(reader, cancellationToken);

        return new OfxParseResult(bankAccount, transactions);
    }

    /// <summary>
    /// Асинхронно получает информацию о банке и счете из OFX-файла
    /// </summary>
    private static async Task<OfxBankAccountDto> GetBankAccountInfoAsync(XmlReader reader)
    {
        string? bankId = null;
        string? accountId = null;

        while (await reader.ReadAsync() && reader.Name != "BANKACCTFROM"){}
        
        while (await reader.ReadAsync())
        {
            switch (reader.NodeType, reader.Name)
            {
                case (XmlNodeType.Element, "BANKID"):
                    bankId = await reader.ReadElementTextAndStayOnEndTagAsync();
                    break;
                case (XmlNodeType.Element, "ACCTID"):
                    accountId = await reader.ReadElementTextAndStayOnEndTagAsync();
                    break;
                case (XmlNodeType.EndElement, "BANKACCTFROM"):
                    return new OfxBankAccountDto(
                        bankId    ?? throw new InvalidOperationException("Не найден BANKID в секции BANKACCTFROM"),
                        accountId ?? throw new InvalidOperationException("Не найден ACCTID в секции BANKACCTFROM")
                        );
            }
        }
        throw new InvalidOperationException("Секция BANKACCTFROM не найдена или не завершена");
    }
    
    /// <summary>
    /// Асинхронно получает транзакции из OFX-файла в виде ленивой async коллекции
    /// </summary>
    private async IAsyncEnumerable<OfxTransactionDto> GetTransactionsAsync(XmlReader reader, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await reader.ReadAsync())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "STMTTRN")
            {
                continue;
            }
            
            string? fitId = null;
            string? dtPosted = null;
            string? memo = null;
            string? name = null;
            string? trnAmt = null;

            while (await reader.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                switch (reader.NodeType, reader.Name)
                {
                    case (XmlNodeType.Element, "FITID"):
                        fitId = await reader.ReadElementTextAndStayOnEndTagAsync();
                        break;
                    case (XmlNodeType.Element, "DTPOSTED"):
                        dtPosted = await reader.ReadElementTextAndStayOnEndTagAsync();
                        break;
                    case (XmlNodeType.Element, "MEMO"):
                        memo = await reader.ReadElementTextAndStayOnEndTagAsync();
                        break;
                    case (XmlNodeType.Element, "NAME"):
                        name = await reader.ReadElementTextAndStayOnEndTagAsync();
                        break;
                    case (XmlNodeType.Element, "TRNAMT"):
                        trnAmt = await reader.ReadElementTextAndStayOnEndTagAsync();
                        break;
                    case (XmlNodeType.EndElement, "STMTTRN"):
                        if (string.IsNullOrWhiteSpace(fitId))
                        {
                            _logger.LogWarning("Пропущена транзакция: отсутствует FITID");
                            break;
                        }
                        if (!TryParseOfxDate(dtPosted, fitId, out var parsedDate))
                        {
                            _logger.LogWarning("Пропущена транзакция с FITID={FitId}: некорректная дата '{DtPosted}'", fitId, dtPosted);
                            break;
                        }
                        if (!decimal.TryParse(trnAmt, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedAmount))
                        {
                            _logger.LogWarning("Пропущена транзакция с FITID={FitId}: некорректная сумма '{TrnAmt}'", fitId, trnAmt);
                            break;
                        }                        
                        if (string.IsNullOrWhiteSpace(memo))
                        {
                            _logger.LogWarning("Пропущена транзакция с FITID={FitId}: отсутствует MEMO", fitId);
                            break;
                        }                        
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            _logger.LogWarning("Пропущена транзакция с FITID={FitId}: отсутствует NAME", fitId);
                            break;
                        }
                        
                        yield return new OfxTransactionDto(
                            TranId: fitId,
                            TranDate: parsedDate,
                            Category: memo,
                            Description: name,
                            Amount: parsedAmount
                        );
                        break;
                }
                if (reader is { NodeType: XmlNodeType.EndElement, Name: "STMTTRN" })
                {
                    break;
                }
            }
        }
    }

    private static readonly string[] OfxDateFormats = ["yyyyMMddHHmmss.fff", "yyyyMMddHHmmss"];

    /// <summary>
    /// Пробует разобрать дату OFX в нескольких форматах.
    /// </summary>
    private bool TryParseOfxDate(string? dtPostedRaw, string? fitId, out DateTime parsedDate)
    {
        parsedDate = default;
        if (string.IsNullOrWhiteSpace(dtPostedRaw))
        {
            _logger.LogWarning("Значение DTPOSTED отсутствует или пустое для FITID: {FitId}", fitId);
            return false;
        }
        foreach (var format in OfxDateFormats)
        {
            if (dtPostedRaw.Length < format.Length)
            {
                continue;
            }
            var candidate = dtPostedRaw[..format.Length];
            if (!DateTime.TryParseExact(candidate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                continue;
            }
            _logger.LogDebug("Дата разобрана: {ParsedDate} (формат {Format})", parsed, format);
            parsedDate = parsed;
            return true;
        }
        _logger.LogWarning("Не удалось разобрать дату '{DtPostedRaw}' для FITID: {FitId}", dtPostedRaw, fitId);
        return false;
    }
}
