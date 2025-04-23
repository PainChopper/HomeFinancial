using HomeFinancial.Application.Common;
using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.OfxParser;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Обработчик сценария импорта OFX-файла
/// </summary>
public class ImportOfxFileHandler : IImportOfxFileHandler
{
    private readonly IOfxParser _parser;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IFileRepository _fileRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger _logger;
    private readonly ImportSettings _importSettings;
    private readonly IValidator<OfxTransaction> _transactionValidator;

    public ImportOfxFileHandler(
        IOfxParser parser,
        ITransactionRepository transactionRepository,
        IFileRepository fileRepository,
        ICategoryRepository categoryRepository,
        ILogger<ImportOfxFileHandler> logger,
        IOptions<ImportSettings> importSettings,
        IValidator<OfxTransaction> transactionValidator)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _importSettings = importSettings.Value ?? throw new ArgumentNullException(nameof(importSettings));
        _transactionValidator = transactionValidator ?? throw new ArgumentNullException(nameof(transactionValidator));
    }

    /// <summary>
    /// Импортирует OFX-файл
    /// </summary>
    public async Task HandleAsync(ImportOfxFileCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Импорт OFX-файла: {FileName}", command.FileName);

        var importedFile = await CreateFile(command, cancellationToken);
        var fileId = importedFile.Id;

        // Пакет транзакций на запись
        var batch = new List<BankTransaction>(_importSettings.BatchSize);

        var totalCount = 0;
        var importedCount = 0;

        // Потоковая обработка транзакций
        var transactions = _parser.ParseOfxFile(command.FileStream);
        foreach (var t in transactions)
        {
            totalCount++;

            // Валидация транзакции через FluentValidation
            var validationResult = await _transactionValidator.ValidateAsync(t, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorList = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Транзакция [{TranId}] пропущена: {Errors}", t.TranId, errorList);
                continue;
            }

            var income = new BankTransaction()
            {
                ImportedFileId = fileId,
                FitId = t.TranId!,
                Date = DateTime.SpecifyKind(t.TranDate!.Value, DateTimeKind.Utc),
                Amount = t.Amount!.Value,
                Description = t.Description!,
                CategoryId = null // Категория будет назначена позже
            };
            
            batch.Add(income);

            if (batch.Count < _importSettings.BatchSize)
            {
                continue;
            }
            importedCount += await _transactionRepository.BulkInsertCopyAsync(batch, cancellationToken);
            batch.Clear();
        }

        if (batch.Count > 0)
        {
            importedCount +=await _transactionRepository.BulkInsertCopyAsync(batch, cancellationToken);
        }

        importedFile.Status = ImportedFileStatus.Processed;
        _logger.LogInformation("Импорт OFX-файла {FileName} завершён. Всего транзакций: {TotalCount}, успешно импортировано: {ImportedCount}", command.FileName, totalCount, importedCount);
    }

    /// <summary>
    /// Создаёт запись об импортируемом файле
    /// </summary>
    private async Task<ImportedFile> CreateFile(ImportOfxFileCommand command, CancellationToken cancellationToken)
    {
        var importedFile = await _fileRepository.GetByFileNameAsync(command.FileName);
        
        if (importedFile is not null)
        {
            throw new InvalidOperationException($"Файл с именем '{command.FileName}' уже импортирован и не может быть повторно загружен.");
        }
        
        importedFile = new ImportedFile
        {
            FileName = command.FileName,
            ImportedAt = DateTime.UtcNow,
            Status = ImportedFileStatus.Processing
        };
        
        return await _fileRepository.CreateAsync(importedFile, cancellationToken);
    }
}
