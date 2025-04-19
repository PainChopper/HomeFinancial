using Microsoft.Extensions.Logging;
using Moq;

// Adjust the namespace as per your project structure

namespace HomeFinancial.OfxParser.Tests
{
    [TestFixture]
    public class OfxParserTests
    {
        private Mock<ILogger<OfxParser>> _loggerMock;
        private OfxParser _parser;

        [SetUp]
        public void Setup()
        {
            // Initialize the mock logger
            _loggerMock = new Mock<ILogger<OfxParser>>();

            // Instantiate the OfxParser with the mocked logger
            _parser = new OfxParser(_loggerMock.Object);
        }

        [Test]
        public void ParseOfxFile_WithValidFile_ReturnsTransactions()
        {
            // Arrange
            var filePath = @"C:\Users\Vitales\Downloads\1.ofx";

            // Verify that the file exists before running the test
            Assert.That(File.Exists(filePath), Is.True, $"The file '{filePath}' does not exist.");

            // Act
            var transactions = _parser.ParseOfxFile(filePath);

            // Assert
            Assert.That(transactions, Is.Not.Null, "The returned transactions list should not be null.");
            Assert.That(transactions, Is.Not.Empty, "The returned transactions list should not be empty.");

            // Optionally, assert specific properties of the transactions
            foreach (var transaction in transactions)
            {
                Assert.That(transaction.TranId, Is.Not.Null, "Transaction ID should not be null.");
                Assert.That(transaction.TranDate, Is.Not.EqualTo(DateTime.MinValue), "Transaction date should be parsed correctly.");
                // Add more assertions as needed
            }

            // Optionally, verify that certain log methods were called
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting to parse OFX file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Add more log verifications as needed
        }

        [Test]
        public void ParseOfxFile_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var filePath = @"C:\Users\Vitales\Downloads\nonexistent.ofx";

            // Act & Assert
            var ex = Assert.Throws<FileNotFoundException>(() => _parser.ParseOfxFile(filePath));
            Assert.That(ex.Message, Does.Contain("could not be found"));
        }

        [Test]
        public void ParseOfxFile_WithInvalidFormat_LogsWarning()
        {
            // Arrange
            var filePath = @"C:\Users\Vitales\Downloads\invalid_format.ofx";

            // Ensure the file exists and has invalid content
            Assert.IsTrue(File.Exists(filePath), $"The file '{filePath}' does not exist.");

            // Act
            var transactions = _parser.ParseOfxFile(filePath);

            // Assert
            // Depending on implementation, transactions might be empty or partially filled
            Assert.IsNotNull(transactions, "The returned transactions list should not be null.");

            // Verify that warnings were logged for parsing issues
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to parse date")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.AtLeastOnce);
        }
    }
}
