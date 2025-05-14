using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using Moq;

namespace HomeFinancial.OfxParser.Tests;

/// <summary>
/// Тесты для проверки обработки ошибок и исключительных ситуаций в OfxParser
/// </summary>
public class OfxParserErrorHandlingTests
{
    private readonly OfxParser _parser;

    public OfxParserErrorHandlingTests()
    {
        var loggerMock = new Mock<ILogger<OfxParser>>();
        _parser = new OfxParser(loggerMock.Object);
    }

    [Fact]
    public async Task ParseStatementsAsync_EmptyFile_ShouldThrowException()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<XmlException>(async () => 
            await _parser.ParseStatementsAsync(stream).ToListAsync());
    }

    [Fact]
    public async Task ParseStatementsAsync_InvalidContent_ShouldThrowException()
    {
        #region TestData
        const string invalidContent = "This is not an XML file at all, just random text";
        #endregion
        
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidContent));

        // Act & Assert
        await Assert.ThrowsAsync<XmlException>(async () => 
            await _parser.ParseStatementsAsync(stream).ToListAsync());
    }

    [Fact]
    public async Task  ParseStatementsAsync_MalformedXml_ShouldThrowException()
    {
        #region TestData
        const string malformedXml = """
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
                                <STMTTRN>
                                    <TRNTYPE>DEBIT</TRNTYPE>
                                    <DTPOSTED>20250329180009.000[+3:MSK]</DTPOSTED>
                                    <TRNAMT>-19.0000</TRNAMT>
                                    <FITID>109456664968</FITID>
                                    <NAME>Гражданин A.</NAME>
                                    <MEMO>Оплата</MEMO>
                                </STMTTRN>
                            <!-- Отсутствующий закрывающий тег BANKTRANLIST -->
                        </STMTRS>
                    </STMTTRNRS>
                </BANKMSGSRSV1>
            </OFX>
            """;
        #endregion

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(malformedXml));

        // Act & Assert
        await Assert.ThrowsAsync<XmlException>(async () => 
            await _parser.ParseStatementsAsync(stream).ToListAsync());
    }

    [Fact]
    public async Task  ParseStatementsAsync_MissingRequiredTag_ShouldThrowInvalidDataException()
    {
        #region TestData
        const string xmlMissingTag = """
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
                            <!-- Отсутствует BANKACCTFROM -->
                            <BANKTRANLIST>
                                <DTSTART>20250301030000.000[+3:MSK]</DTSTART>
                                <DTEND>20250401025959.000[+3:MSK]</DTEND>
                                <STMTTRN>
                                    <TRNTYPE>DEBIT</TRNTYPE>
                                    <DTPOSTED>20250329180009.000[+3:MSK]</DTPOSTED>
                                    <TRNAMT>-19.0000</TRNAMT>
                                    <FITID>109456664968</FITID>
                                    <NAME>Гражданин A.</NAME>
                                    <MEMO>Оплата</MEMO>
                                </STMTTRN>
                            </BANKTRANLIST>
                        </STMTRS>
                    </STMTTRNRS>
                </BANKMSGSRSV1>
            </OFX>
            """;
        #endregion

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlMissingTag));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(async () => 
            await _parser.ParseStatementsAsync(stream).ToListAsync());
    }
}
