using System.Text;
using Microsoft.Extensions.Logging;
using Moq;

namespace HomeFinancial.OfxParser.Tests;

/// <summary>
/// Базовые тесты функциональности OfxParser
/// </summary>
public class OfxParserBasicTests
{
    private readonly OfxParser _parser = new(new Mock<ILogger<OfxParser>>().Object);

    [Fact]
    public async Task ParseStatementsAsync_ValidOfxWithMultipleAccounts_ShouldReturnAllAccounts()
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
                                    <NAME>Иванов Иван Иванович</NAME>
                                    <MEMO>Оплата</MEMO>
                                </STMTTRN>
                            </BANKTRANLIST>
                        </STMTRS>
                    </STMTTRNRS>
                    <STMTTRNRS>
                        <TRNUID>0</TRNUID>
                        <STATUS><CODE>0</CODE><SEVERITY>INFO</SEVERITY></STATUS>
                        <STMTRS>
                            <CURDEF>RUB</CURDEF>
                            <BANKACCTFROM>
                                <BANKID>B-BANK</BANKID>
                                <ACCTID>40817810XXXX123456</ACCTID>
                                <ACCTTYPE>SAVINGS</ACCTTYPE>
                            </BANKACCTFROM>
                            <BANKTRANLIST>
                                <DTSTART>20250301030000.000[+3:MSK]</DTSTART>
                                <DTEND>20250401025959.000[+3:MSK]</DTEND>
                                <STMTTRN>
                                    <TRNTYPE>CREDIT</TRNTYPE>
                                    <DTPOSTED>20250328222759.000[+3:MSK]</DTPOSTED>
                                    <TRNAMT>110.2800</TRNAMT>
                                    <FITID>109362350911</FITID>
                                    <NAME>Петров Петр Петрович</NAME>
                                    <MEMO>Услуги</MEMO>
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
        var statements = await _parser.ParseStatementsAsync(stream).ToListAsync();

        // Assert
        Assert.Equal(2, statements.Count);
        
        // Проверка первого счета
        var firstAccount = statements[0];
        Assert.Equal("A-BANK", firstAccount.BankId);
        Assert.Equal("30101810ZZZZ987654", firstAccount.BankAccountId);
        Assert.Equal("CHECKING", firstAccount.BankAccountType);
        
        // Проверка второго счета
        var secondAccount = statements[1];
        Assert.Equal("B-BANK", secondAccount.BankId);
        Assert.Equal("40817810XXXX123456", secondAccount.BankAccountId);
        Assert.Equal("SAVINGS", secondAccount.BankAccountType);
    }

    [Fact]
    public async Task ParseStatementsAsync_NoTransactions_ShouldReturnEmptyTransactionsList()
    {
        #region TestData
        const string xmlNoTransactions = """
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
                                <!-- Нет транзакций -->
                            </BANKTRANLIST>
                        </STMTRS>
                    </STMTTRNRS>
                </BANKMSGSRSV1>
            </OFX>
            """;
        #endregion

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlNoTransactions));

        // Act
        var statements = await _parser.ParseStatementsAsync(stream).ToListAsync();
        var transactions = await statements[0].Transactions.ToListAsync();

        // Assert
        Assert.Single(statements);
        Assert.Empty(transactions);
    }
}
