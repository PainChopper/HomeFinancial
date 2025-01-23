using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.OfxParser;

public class OfxParser
{
    private readonly ILogger<OfxParser> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OfxParser"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public OfxParser(ILogger<OfxParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Reads an OFX file and returns a list of transactions.
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>List of OfxTransaction</returns>
    public List<OfxTransaction> ParseOfxFile(string filePath)
    {
        _logger.LogInformation("Starting to parse OFX file: {FilePath}", filePath);

        // Load the document
        // Important: If the OFX contains separate "<?xml?>" chunks, they can sometimes break XDocument.
        // Usually, loading the file directly (XDocument.Load) is sufficient.
        // If there's a problem with headers, they can be manually removed beforehand.
        XDocument xdoc;
        try
        {
            xdoc = XDocument.Load(filePath);
            _logger.LogInformation("OFX document loaded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading OFX document: {ErrorMessage}", ex.Message);
            throw;
        }

        // Find all <STMTTRN> nodes
        // Descendants("STMTTRN") retrieves all elements named STMTTRN at any level of nesting
        var stmtTrnElements = xdoc.Descendants("STMTTRN");
        int transactionCount = stmtTrnElements.Count();
        _logger.LogInformation("Found {Count} transaction elements.", transactionCount);

        var transactions = new List<OfxTransaction>();

        foreach (var element in stmtTrnElements)
        {
            // Read fields from XML
            var fitIdValue = element.Element("FITID")?.Value;
            var dtPostedRaw = element.Element("DTPOSTED")?.Value;
            var memoValue = element.Element("MEMO")?.Value;
            var nameValue = element.Element("NAME")?.Value;
            var trnAmtValue = element.Element("TRNAMT")?.Value;

            _logger.LogDebug("Parsing transaction FITID: {FitId}", fitIdValue);

            // Parse the date
            // Format is approximately "20250113224820.000[+3:MSK]".
            // We need to discard everything after the dot or before the square brackets.
            // For example:
            //   "20250113224820.000[+3:MSK]" -> "20250113224820.000"
            // Then use DateTime.ParseExact with the format "yyyyMMddHHmmss.fff".
            DateTime parsedDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(dtPostedRaw))
            {
                // Split at the first '[' if it exists
                var split = dtPostedRaw.Split('[');
                var datePart = split[0]; // "20250113224820.000"

                try
                {
                    parsedDate = DateTime.ParseExact(
                        datePart,
                        "yyyyMMddHHmmss.fff",
                        CultureInfo.InvariantCulture
                    );
                    _logger.LogDebug("Parsed date with milliseconds: {ParsedDate}", parsedDate);
                }
                catch (FormatException)
                {
                    // If parsing with milliseconds fails, try without milliseconds
                    try
                    {
                        parsedDate = DateTime.ParseExact(
                            datePart,
                            "yyyyMMddHHmmss",
                            CultureInfo.InvariantCulture
                        );
                        _logger.LogDebug("Parsed date without milliseconds: {ParsedDate}", parsedDate);
                    }
                    catch (FormatException ex)
                    {
                        // If still failing, log the issue
                        _logger.LogWarning("Failed to parse date '{DtPostedRaw}': {ErrorMessage}", dtPostedRaw, ex.Message);
                        // Optionally, you can choose to continue or throw an exception
                    }
                }
            }
            else
            {
                _logger.LogWarning("DTPOSTED value is null or empty for FITID: {FitId}", fitIdValue);
            }

            // Parse the amount
            decimal parsedAmount = 0;
            if (decimal.TryParse(trnAmtValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt))
            {
                parsedAmount = amt;
                _logger.LogDebug("Parsed amount: {Amount}", parsedAmount);
            }
            else
            {
                _logger.LogWarning("Failed to parse amount: {TrnAmtValue} for FITID: {FitId}", trnAmtValue, fitIdValue);
            }

            // Create DTO and add to the list
            var transaction = new OfxTransaction
            {
                TranId = fitIdValue,
                TranDate = parsedDate,
                Category = memoValue,
                Description = nameValue,
                Amount = parsedAmount
            };

            transactions.Add(transaction);
            _logger.LogDebug("Added transaction ID: {TranId}", transaction.TranId);
        }

        _logger.LogInformation("Finished parsing OFX file. Total transactions parsed: {Count}", transactions.Count);
        return transactions;
    }
}

// Assuming the OfxTransaction class is defined as follows:
public class OfxTransaction
{
    public string TranId { get; set; }
    public DateTime TranDate { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
}