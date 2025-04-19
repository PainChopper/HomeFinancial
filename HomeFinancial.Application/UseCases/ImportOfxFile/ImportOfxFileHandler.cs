using Microsoft.Extensions.Logging;
using HomeFinancial.OfxParser;
using HomeFinancial.Domain.Repositories;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Обработчик сценария импорта OFX-файла
/// </summary>
public class ImportOfxFileHandler : IImportOfxFileHandler
{
    private readonly IOfxParser _parser;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IFileRepository _fileRepository;
    private readonly ILogger<ImportOfxFileHandler> _logger;

    public ImportOfxFileHandler(
        IOfxParser parser,
        ITransactionRepository transactionRepository,
        IFileRepository fileRepository,
        ILogger<ImportOfxFileHandler> logger)
    {
        _parser = parser;
        _transactionRepository = transactionRepository;
        _fileRepository = fileRepository;
        _logger = logger;
    }

    /// <summary>
    /// Импортирует OFX-файл
    /// </summary>
    public async Task HandleAsync(ImportOfxFileCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Импорт OFX-файла: {FileName}", command.FileName);

        // Парсинг транзакций из потока
        var transactions = _parser.ParseOfxFile(command.FileStream);
        _logger.LogInformation("Транзакций получено: {Count}", transactions.Count);

        // TODO: Сохранить транзакции в базу данных через ITransactionRepository
        // await _transactionRepository.AddRangeAsync(transactions, cancellationToken);
        // TODO: Сохранить информацию о файле через IFileRepository
        // await _fileRepository.AddAsync(..., cancellationToken);

        await Task.CompletedTask;
    }
}
