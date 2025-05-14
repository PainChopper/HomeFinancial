using FluentValidation;
using HomeFinancial.Application.Common;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.OfxParser;
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
    private readonly IEntryCategoryRepository _entryCategoryRepository;
    private readonly ILogger _logger;
    private readonly ImportSettings _importSettings;
    private readonly IValidator<OfxTransactionDto> _transactionValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ITransactionInserter _transactionInserter;
    private readonly ILeaseService _leaseService;

    public ImportOfxFileHandler(
        IOfxParser parser,
        IFileRepository fileRepository,
        IEntryCategoryRepository entryCategoryRepository,
        ILogger<ImportOfxFileHandler> logger,
        IOptions<ImportSettings> importSettings,
        IValidator<OfxTransactionDto> transactionValidator,
        IDateTimeProvider dateTimeProvider,
        ITransactionInserter transactionInserter,
        ILeaseService leaseService)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        _entryCategoryRepository = entryCategoryRepository ?? throw new ArgumentNullException(nameof(entryCategoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _importSettings = importSettings.Value ?? throw new ArgumentNullException(nameof(importSettings));
        _transactionValidator = transactionValidator ?? throw new ArgumentNullException(nameof(transactionValidator));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _transactionInserter = transactionInserter ?? throw new ArgumentNullException(nameof(transactionInserter));
        _leaseService = leaseService ?? throw new ArgumentNullException(nameof(leaseService));
    }
    
    /// <inheritdoc />
    public async Task<ApiResponse<ImportOfxFileResult>> HandleAsync(ImportOfxFileCommand command, CancellationToken ct)
    {
        return await Task.FromResult(new ApiResponse<ImportOfxFileResult>(true));
        /*
        _logger.LogInformation("Импорт OFX-файла: {FileName}", command.FileName);

        var leaseId = await _leaseService.AcquireLeaseAsync(command.FileName, TimeSpan.FromMinutes(1));
        
        var importedFile = await CreateFile(command.FileName, ct, leaseId);

        // Пакет DTO для пакетной вставки
        var batch = new List<TransactionInsertDto>(_importSettings.BatchSize);

        var totalCount = 0;
        var importedCount = 0;
        var duplicatesCount = 0;
        var errorCount = 0;

        // Потоковая обработка транзакций
        var result = await _parser.ParseOfxFileAsync(command.FileStream, ct);
        var transactions =  await result.Statements.SelectMany(s => s.Transactions;

        await foreach (var t in transactions.WithCancellation(ct))
        {
            totalCount++;

            // Валидация транзакции через FluentValidation
            var validationResult = await _transactionValidator.ValidateAsync(t, ct);
            if (!validationResult.IsValid)
            {
                var errorList = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Транзакция [{Id}] пропущена: {Errors}", t.Id, errorList);
                errorCount++;
                continue;
            }

            // Определяем или создаём категорию и формируем DTO
            var categoryId = await _entryCategoryRepository.GetOrCreateCategoryIdAsync(t.Category);
            var dto = new TransactionInsertDto(
                FileId: importedFile.Id,
                Id: t.Id,
                Date: DateTime.SpecifyKind(t.TranDate, DateTimeKind.Utc),
                Amount: t.Amount,
                Description: t.Description,
                CategoryId: categoryId
            );

            batch.Add(dto);

            if (batch.Count < _importSettings.BatchSize)
            {
                continue;
            }
            await _leaseService.ValidateAndExtendLeaseAsync(command.FileName, leaseId, TimeSpan.FromMinutes(1));
            var bulkResult = await _transactionInserter.BulkInsertCopyAsync(batch, ct);
            importedCount += bulkResult.Inserted;
            duplicatesCount += bulkResult.Duplicates;
            batch.Clear();
        }

        if (batch.Count > 0)
        {
            await _leaseService.ValidateAndExtendLeaseAsync(command.FileName, leaseId, TimeSpan.FromMinutes(1));
            var bulkResult = await _transactionInserter.BulkInsertCopyAsync(batch, ct);
            importedCount += bulkResult.Inserted;
            duplicatesCount += bulkResult.Duplicates;
        }

        importedFile.Status = BankFileStatus.Completed;
        await _fileRepository.UpdateAsync(importedFile, ct);
        await _leaseService.ReleaseLeaseAsync(command.FileName, leaseId);
        
        _logger.LogInformation("Импорт OFX-файла {FileName} завершён. Всего транзакций: {TotalCount}, успешно импортировано: {ImportedCount}", command.FileName, totalCount, importedCount);

        return new ApiResponse<ImportOfxFileResult>(true, new ImportOfxFileResult { TotalCount = totalCount, ImportedCount = importedCount, ErrorCount = errorCount, SkippedDuplicateCount = duplicatesCount });
    */
    }

    /// <summary>
    /// Создаёт запись об импортируемом файле
    /// </summary>
    private async Task<BankFile> CreateFile(string fileName,
        CancellationToken ct, Guid leaseId)
    {
        var importedFile = await _fileRepository.GetByFileNameAsync(fileName);

        
        if (importedFile != null)
        {
            if(importedFile.Status == BankFileStatus.Completed)
            {
                await _leaseService.ReleaseLeaseAsync(importedFile.FileName, leaseId);
                throw new InvalidOperationException(
                    $"Файл с именем '{importedFile.FileName}' уже успешно импортирован и не может быть повторно загружен.");
            }

            _logger.LogWarning("Удаление ранее импортированного файла с именем '{FileName}' и статусом {Status}", importedFile.FileName, importedFile.Status);
            await _fileRepository.DeleteAsync(importedFile.Id, ct);
        }
        
        importedFile = new BankFile
        {
            FileName = fileName,
            ImportedAt = _dateTimeProvider.UtcNow,
            Status = BankFileStatus.InProgress
        };
        
        return await _fileRepository.CreateAsync(importedFile, ct);
    }
}
