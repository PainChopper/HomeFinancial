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
    private readonly IDateTimeProvider _dateTimeProvider;

    public ImportOfxFileHandler(
        IOfxParser parser,
        ITransactionRepository transactionRepository,
        IFileRepository fileRepository,
        ICategoryRepository categoryRepository,
        ILogger<ImportOfxFileHandler> logger,
        IOptions<ImportSettings> importSettings,
        IValidator<OfxTransaction> transactionValidator,
        IDateTimeProvider dateTimeProvider)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _importSettings = importSettings.Value ?? throw new ArgumentNullException(nameof(importSettings));
        _transactionValidator = transactionValidator ?? throw new ArgumentNullException(nameof(transactionValidator));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }
    
    /// <inheritdoc />
    public async Task<ApiResponse<ImportOfxFileResult>> HandleAsync(ImportOfxFileCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Импорт OFX-файла: {FileName}", command.FileName);

        var importedFile = await CreateFile(command, cancellationToken);
        var fileId = importedFile.Id;

        // Пакет транзакций на запись
        var batch = new List<BankTransaction>(_importSettings.BatchSize);

        var totalCount = 0;
        var importedCount = 0;
        var errorCount = 0; // Количество ошибочных транзакций

        // Потоковая обработка транзакций
        var transactions = _parser.ParseOfxFile(command.FileStream);
        var skippedDuplicateCount = 0;
        foreach (var t in transactions)
        {
            totalCount++;

            // Валидация транзакции через FluentValidation
            var validationResult = await _transactionValidator.ValidateAsync(t, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorList = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Транзакция [{TranId}] пропущена: {Errors}", t.TranId, errorList);
                errorCount++;
                continue;
            }

            // Определяем или создаём категорию
            var category = await _categoryRepository.GetOrCreateAsync(t.Category!, cancellationToken);
            var income = new BankTransaction
            {
                ImportedFile = importedFile,
                FitId = t.TranId!,
                Date = DateTime.SpecifyKind(t.TranDate!.Value, DateTimeKind.Utc),
                Amount = t.Amount!.Value,
                Description = t.Description!,
                Category = category
            };

            batch.Add(income);

            if (batch.Count < _importSettings.BatchSize)
            {
                continue;
            }
            var bulkResult = await _transactionRepository.BulkInsertCopyAsync(batch, cancellationToken);
            importedCount += bulkResult.InsertedCount;
            skippedDuplicateCount += bulkResult.SkippedDuplicateCount;
            batch.Clear();
        }

        if (batch.Count > 0)
        {
            var bulkResult = await _transactionRepository.BulkInsertCopyAsync(batch, cancellationToken);
            importedCount += bulkResult.InsertedCount;
            skippedDuplicateCount += bulkResult.SkippedDuplicateCount;
        }

        importedFile.Status = ImportedFileStatus.Processed;
        _logger.LogInformation("Импорт OFX-файла {FileName} завершён. Всего транзакций: {TotalCount}, успешно импортировано: {ImportedCount}", command.FileName, totalCount, importedCount);
        return new ApiResponse<ImportOfxFileResult>(true, new ImportOfxFileResult { TotalCount = totalCount, ImportedCount = importedCount, ErrorCount = errorCount, SkippedDuplicateCount = skippedDuplicateCount });
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
            ImportedAt = _dateTimeProvider.UtcNow,
            Status = ImportedFileStatus.Processing
        };
        
        return await _fileRepository.CreateAsync(importedFile, cancellationToken);
    }
}
