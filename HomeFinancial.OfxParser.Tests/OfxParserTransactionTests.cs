using System.Text;
using Microsoft.Extensions.Logging;
using Moq;

namespace HomeFinancial.OfxParser.Tests;

/// <summary>
/// Тесты для проверки функциональности обработки транзакций OfxParser
/// </summary>
public class OfxParserTransactionTests
{
    private readonly Mock<ILogger<OfxParser>> _loggerMock;
    private readonly OfxParser _parser;

    public OfxParserTransactionTests()
    {
        _loggerMock = new Mock<ILogger<OfxParser>>();
        _parser = new OfxParser(_loggerMock.Object);
    }

    [Fact]
    public async Task ParseStatementsAsync_ValidOfx_ShouldReturnCorrectTransactions()
    {
        #region TestData
        const string xml = """
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
                                <STMTTRN>
                                    <TRNTYPE>CREDIT</TRNTYPE>
                                    <DTPOSTED>20250321001239.000[+3:MSK]</DTPOSTED>
                                    <TRNAMT>67000.0000</TRNAMT>
                                    <FITID>108522034753</FITID>
                                    <NAME>Кафе "Лето"</NAME>
                                    <MEMO>Возврат</MEMO>
                                </STMTTRN>
                            </BANKTRANLIST>
                        </STMTRS>
                    </STMTTRNRS>
                </BANKMSGSRSV1>
            </OFX>
            """;
        #endregion

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        // Act
        var transactions = await _parser.ParseStatementsAsync(stream)
	        .SelectMany(stmt => stmt.Transactions)
	        .ToListAsync();

        // Assert
        Assert.Equal(2, transactions.Count);
        
        // Проверка первой транзакции
        var firstTransaction = transactions[0];
        Assert.Equal("109456664968", firstTransaction.Id);
        Assert.Equal("DEBIT", firstTransaction.TranType);
        Assert.Equal(-19.0000m, firstTransaction.Amount);
        Assert.Equal(new DateTime(2025, 3, 29, 18, 0, 9), firstTransaction.TranDate);
        Assert.Equal("Гражданин A.", firstTransaction.Description);
        Assert.Equal("Оплата", firstTransaction.Category);
        
        // Проверка второй транзакции
        var secondTransaction = transactions[1];
        Assert.Equal("108522034753", secondTransaction.Id);
        Assert.Equal("CREDIT", secondTransaction.TranType);
        Assert.Equal(67000.0000m, secondTransaction.Amount);
        Assert.Equal(new DateTime(2025, 3, 21, 0, 12, 39), secondTransaction.TranDate);
        Assert.Equal("Кафе \"Лето\"", secondTransaction.Description);
        Assert.Equal("Возврат", secondTransaction.Category);
    }

    [Fact]
    public async Task ParseStatementsAsync_InvalidTransactionData_ShouldSkipInvalidTransactions()
    {
        #region TestData
        const string xmlInvalidTransactions = """
            <?xml version='1.0' encoding='utf-8'?>
            <OFX>
            	<SIGNONMSGSRSV1>
            		<SONRS>
            			<STATUS>
            				<CODE>0</CODE>
            				<SEVERITY>INFO</SEVERITY>
            			</STATUS>
            			<DTSERVER>20250331183147.017[+3:MSK]</DTSERVER>
            			<LANGUAGE>RUS</LANGUAGE>
            		</SONRS>
            	</SIGNONMSGSRSV1>
            	<BANKMSGSRSV1>
            		<STMTTRNRS>
            			<TRNUID>0</TRNUID>
            			<STATUS>
            				<CODE>0</CODE>
            				<SEVERITY>INFO</SEVERITY>
            			</STATUS>
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
            						<TRNAMT>notanumber</TRNAMT>
            						<FITID>109456664968</FITID>
            						<NAME>Гражданин A.</NAME>
            						<MEMO>Оплата</MEMO>
            					</STMTTRN>
            					<STMTTRN>
            						<TRNTYPE>CREDIT</TRNTYPE>
            						<DTPOSTED>invalid</DTPOSTED>
            						<TRNAMT>67000.0000</TRNAMT>
            						<FITID>108522034753</FITID>
            						<NAME>Кафе "Лето"</NAME>
            						<MEMO>Возврат</MEMO>
            					</STMTTRN>
            					<STMTTRN>
            						<TRNTYPE>DEBIT</TRNTYPE>
            						<DTPOSTED>20250329180009.000[+3:MSK]</DTPOSTED>
            						<TRNAMT>-19.0000</TRNAMT>
            						<FITID>109456664969</FITID>
            						<NAME>Гражданин Б.</NAME>
            						<MEMO>Оплата</MEMO>
            					</STMTTRN>
            				</BANKTRANLIST>
            			</STMTRS>
            		</STMTTRNRS>
            	</BANKMSGSRSV1>
            </OFX>
            """;
        #endregion

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlInvalidTransactions));

        // Act
        var transactions = await _parser.ParseStatementsAsync(stream)
	        .SelectMany(stmt => stmt.Transactions)
	        .ToListAsync();
        
        // Assert
        Assert.Single(transactions);
        Assert.Equal("109456664969", transactions[0].Id);
        
        // Проверка логирования пропущенных транзакций
        _loggerMock.VerifyWarningContains("некорректная сумма");
        
        _loggerMock.VerifyWarningContains("некорректная дата");
    }

    [Fact]
    public async Task ParseStatementsAsync_CancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var xml = TestHelpers.GenerateOfxWithManyTransactions(1000); // Много транзакций для длительной обработки
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        
        var cts = new CancellationTokenSource();
        cts.CancelAfter(10); // Отменить через 10 мс

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () => 
	        // ReSharper disable once MethodSupportsCancellation
	        await _parser.ParseStatementsAsync(stream, cts.Token).ToListAsync());
    }
}
