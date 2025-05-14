using System.Text;
using Microsoft.Extensions.Logging;
using Moq;

namespace HomeFinancial.OfxParser.Tests;

/// <summary>
/// Вспомогательные методы для тестирования
/// </summary>
public static class TestHelpers
{
 
    /// <summary>
    /// Проверяет, что мок <see cref="ILogger"/> получил вызов <c>Log</c> с уровнем <c>Warning</c>
    /// и что в итоговом сообщении содержится заданная подстрока.
    /// </summary>
    /// <param name="loggerMock">Мок <see cref="ILogger"/>.</param>
    /// <param name="substring">Подстрока, которую должно содержать сообщение.</param>
    /// <param name="times">Ожидаемое количество вызовов.</param>
    public static void VerifyWarningContains(
        this Mock<ILogger<OfxParser>> loggerMock,
        string substring)
    {
        loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(substring)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    /// <summary>
    /// Генерирует XML-файл с указанным количеством транзакций
    /// </summary>
    public static string GenerateOfxWithManyTransactions(int count)
    {
        // Используем Raw String Literals для объявления шаблона XML
        const string template = """
                                <?xml version='1.0' encoding='utf-8'?>
                                <OFX>
                                    <SIGNONMSGSRSV1>
                                        <SONRS>
                                            <STATUS><CODE>0</CODE><SEVERITY>INFO</SEVERITY></STATUS>
                                            <DTSERVER>20250331183147.017[+3:MSK]</DTSERVER>
                                            <LANGUAGE>RUS</LANGUAGE>
                                        </SONRS>
                                    </SIGNONMSGSRSV1>
                                    <BANKMSGSRSV1>
                                        <STMTTRNRS>
                                            <TRNUID>0</TRNUID>
                                            <STATUS><CODE>0</CODE><SEVERITY>INFO</SEVERITY></STATUS>
                                            <STMTRS>
                                                <CURDEF>RUB</CURDEF>
                                                <BANKACCTFROM>
                                                    <BANKID>A-BANK</BANKID>
                                                    <ACCTID>30101810ZZZZ987654</ACCTID>
                                                    <ACCTTYPE>CHECKING</ACCTTYPE>
                                                </BANKACCTFROM>
                                                <BANKTRANLIST>
                                                    <DTSTART>20250301030000.000[+3:MSK]</DTSTART>
                                                    <DTEND>20250401025959.000[+3:MSK]</DTEND>
                                """;
            
        var transactionsXml = new StringBuilder();
        
        // Формируем XML для каждой транзакции
        for (var i = 0; i < count; i++)
        {
            var type = i % 2 == 0 ? "DEBIT" : "CREDIT";
            var amount = (i % 2 == 0 ? -1 : 1) * (i * 10.5);
            var time = $"20250329{i % 24:D2}{i % 60:D2}{i % 60:D2}";
            var id = $"109456{i:D6}";
            
            var transaction = $"""
                                <STMTTRN>
                                    <TRNTYPE>{type}</TRNTYPE>
                                    <DTPOSTED>{time}.000[+3:MSK]</DTPOSTED>
                                    <TRNAMT>{amount}</TRNAMT>
                                    <FITID>{id}</FITID>
                                    <NAME>Контрагент {i}</NAME>
                                    <MEMO>Операция {i}</MEMO>
                                </STMTTRN>
            """;
            
            transactionsXml.Append(transaction);
        }
        
        const string footer = """
                          </BANKTRANLIST>
                      </STMTRS>
                  </STMTTRNRS>
              </BANKMSGSRSV1>
          </OFX>
          """;
        
        return new StringBuilder()
            .Append(template)
            .Append(transactionsXml)
            .Append(footer)
            .ToString();
    }
}
