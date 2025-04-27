using HomeFinancial.Application.Common;
using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.OfxParser;
using FluentValidation;
using HomeFinancial.Application.Dtos;
using HomeFinancial.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Обработчик сценария импорта OFX-файла
/// </summary>
public class ImportOfxFileHandler : IImportOfxFileHandler
{
    private readonly IOfxParser _parser;
    private readonly IFileRepository _fileRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger _logger;
    private readonly ImportSettings _importSettings;
    private readonly IValidator<OfxTransaction> _transactionValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ITransactionInserter _transactionInserter;

    public ImportOfxFileHandler(
        IOfxParser parser,
        IFileRepository fileRepository,
        ICategoryRepository categoryRepository,
        ILogger<ImportOfxFileHandler> logger,
        IOptions<ImportSettings> importSettings,
        IValidator<OfxTransaction> transactionValidator,
        IDateTimeProvider dateTimeProvider,
        ITransactionInserter transactionInserter)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _importSettings = importSettings.Value ?? throw new ArgumentNullException(nameof(importSettings));
        _transactionValidator = transactionValidator ?? throw new ArgumentNullException(nameof(transactionValidator));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _transactionInserter = transactionInserter ?? throw new ArgumentNullException(nameof(transactionInserter));
    }
    
    /// <inheritdoc />
    public async Task<ApiResponse<ImportOfxFileResult>> HandleAsync(ImportOfxFileCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Импорт OFX-файла: {FileName}", command.FileName);

        var importedFile = await CreateFile(command, cancellationToken);
        var fileId = importedFile.Id;

        // Пакет DTO для пакетной вставки
        var batch = new List<TransactionInsertDto>(_importSettings.BatchSize);

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

            // Определяем или создаём категорию и формируем DTO
            var categoryId = await _categoryRepository.GetOrCreateCategoryIdAsync(t.Category!);
            var dto = new TransactionInsertDto(
                FileId: fileId,
                FitId: t.TranId!,
                Date: DateTime.SpecifyKind(t.TranDate!.Value, DateTimeKind.Utc),
                Amount: t.Amount!.Value,
                Description: t.Description!,
                CategoryId: categoryId
            );

            batch.Add(dto);

            if (batch.Count < _importSettings.BatchSize)
            {
                continue;
            }
            var bulkResult = await _transactionInserter.BulkInsertCopyAsync(batch, cancellationToken);
            importedCount += bulkResult.InsertedCount;
            skippedDuplicateCount += bulkResult.SkippedDuplicateCount;
            batch.Clear();
        }

        if (batch.Count > 0)
        {
            var bulkResult = await _transactionInserter.BulkInsertCopyAsync(batch, cancellationToken);
            importedCount += bulkResult.InsertedCount;
            skippedDuplicateCount += bulkResult.SkippedDuplicateCount;
        }

        importedFile.Status = BankFileStatus.Completed;
        _logger.LogInformation("Импорт OFX-файла {FileName} завершён. Всего транзакций: {TotalCount}, успешно импортировано: {ImportedCount}", command.FileName, totalCount, importedCount);
        return new ApiResponse<ImportOfxFileResult>(true, new ImportOfxFileResult { TotalCount = totalCount, ImportedCount = importedCount, ErrorCount = errorCount, SkippedDuplicateCount = skippedDuplicateCount });
    }

    /// <summary>
    /// Создаёт запись об импортируемом файле
    /// </summary>
    private async Task<BankFile> CreateFile(ImportOfxFileCommand command, CancellationToken cancellationToken)
    {
        if (await _fileRepository.ExistsByFileNameAsync(command.FileName))
        {
            throw new InvalidOperationException($"Файл с именем '{command.FileName}' уже импортирован и не может быть повторно загружен.");
        }
        
        var importedFile = new BankFile
        {
            FileName = command.FileName,
            ImportedAt = _dateTimeProvider.UtcNow,
            Status = BankFileStatus.InProgress
        };
        
        return await _fileRepository.CreateAsync(importedFile, cancellationToken);
    }
}
