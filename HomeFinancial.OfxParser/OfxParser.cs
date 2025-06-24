using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
using HomeFinancial.OfxParser.Dto;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.OfxParser;

public class OfxParser : IOfxParser
{
    private readonly ILogger<OfxParser> _logger;

    public OfxParser(ILogger<OfxParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<OfxAccountStatementDto> ParseStatementsAsync(Stream stream, [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var reader = XmlReader.Create(stream, new XmlReaderSettings {IgnoreWhitespace = true, Async = true });

        await reader.SkipToElementAsync(OfxTag.Ofx, ct);
        await reader.SkipToElementAsync(OfxTag.BankMsgsRsv1, ct);

        var hasStatements  = false;
        
        while (await reader.TrySkipToElementAsync(OfxTag.StmtTrnRs, ct))
        {
            hasStatements = true;
            
            await reader.SkipToElementAsync(OfxTag.StmTrs, ct);
            await reader.SkipToElementAsync(OfxTag.BankAcctFrom, ct);
            var bankId = await reader.ReadElementContentAsStringAsync(OfxTag.BankId, ct);
            var accountId = await reader.ReadElementContentAsStringAsync(OfxTag.AcctId, ct);
            var accountType = await reader.ReadElementContentAsStringAsync(OfxTag.AcctType, ct);

            await reader.SkipToElementAsync(OfxTag.BankTranList, ct);
            
            yield return new OfxAccountStatementDto(
                bankId,
                accountId,
                accountType,
                ReadTransactionsAsync(reader, ct));
        }
        
        if (!hasStatements)
        {
            throw new InvalidDataException($"В банковском файле не найдено секций <{OfxTag.StmtTrnRs}>.");
        }
    }

    /// <summary>
    /// Читает транзакции из банковской выписки.
    /// </summary>
    /// <param name="reader">XML ридер</param>
    /// <param name="ct">Токен отмены операции</param>
    /// <returns>Асинхронный перечислитель транзакций</returns>
    private async IAsyncEnumerable<OfxTransactionDto> ReadTransactionsAsync(XmlReader reader, [EnumeratorCancellation] CancellationToken ct = default)
    {
        while (await reader.TrySkipToElementAsync(OfxTag.StmtTrn, ct))
        {
            var trnType = await reader.ReadElementContentAsStringAsync(OfxTag.TrnType, ct);
            var dtPosted = ParseDate(await reader.ReadElementContentAsStringAsync(OfxTag.DtPosted, ct));
            var trnAmt = ParseAmount(await reader.ReadElementContentAsStringAsync(OfxTag.TrnAmt, ct));
            var fitId = await reader.ReadElementContentAsStringAsync(OfxTag.FitId, ct);
            var name = await reader.ReadElementContentAsStringAsync(OfxTag.Name, ct);
            var memo = await reader.ReadElementContentAsStringAsync(OfxTag.Memo, ct);
            
            if (trnAmt is null)
            {
                _logger.LogWarning("Пропущена транзакция с FITID={Id}: некорректная сумма '{TrnAmt}'", fitId, trnAmt);
                continue;
            }

            if (dtPosted is null)
            {
                _logger.LogWarning("Пропущена транзакция с FITID={Id}: некорректная дата '{DtPosted}'", fitId, dtPosted);
                continue;
            }

            yield return new OfxTransactionDto(
                Id: fitId,
                TranType: trnType,
                TranDate: dtPosted.Value,
                Category: memo,
                Description: name,
                Amount: trnAmt.Value
            );
        }

        yield break;

        decimal? ParseAmount(string amountString)
        {
            return !string.IsNullOrEmpty(amountString) &&
                   decimal.TryParse(
                       amountString,
                       NumberStyles.Number,
                       CultureInfo.InvariantCulture,
                       out var amount)
                ? amount
                : null;
        }

        DateTime? ParseDate(string dateString)
        {
            const string format = "yyyyMMddHHmmss.fff";
            
            if (string.IsNullOrEmpty(dateString) || dateString.Length < format.Length)
            {
                return null;
            }
            
            return DateTime.TryParseExact(dateString[..format.Length],
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var date)
                ? date
                : null;
        }
    }
}