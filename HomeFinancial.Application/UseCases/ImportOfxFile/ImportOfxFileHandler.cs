using Microsoft.Extensions.Logging;
using HomeFinancial.OfxParser;
using HomeFinancial.Domain.Repositories;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Обработчик сценария импорта OFX-файла (шаблон)
/// </summary>
public class ImportOfxFileHandler
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
    /// Импортирует OFX-файл (реализация пока отсутствует)
    /// </summary>
    public async Task HandleAsync(ImportOfxFileCommand command, CancellationToken cancellationToken = default)
    {
        // TODO: Реализовать логику импорта OFX-файла
        _logger.LogInformation("Импорт OFX-файла: {FileName}", command.FileName);
        await Task.CompletedTask;
    }
}
