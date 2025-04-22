using HomeFinancial.Application.Validators;
using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.OfxParser;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<ImportOfxFileHandler> _logger;

    public ImportOfxFileHandler(
        IOfxParser parser,
        ITransactionRepository transactionRepository,
        IFileRepository fileRepository,
        ICategoryRepository categoryRepository,
        ILogger<ImportOfxFileHandler> logger)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Импортирует OFX-файл
    /// </summary>
    public async Task HandleAsync(ImportOfxFileCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Импорт OFX-файла: {FileName}", command.FileName);

        var importedFile = await CreateFile(command, cancellationToken);
        var fileId = importedFile.Id;

        #region  OLD

            
        // Размер пакета для обработки транзакций
        const int batchSize = 100;
            
        // Пакеты для доходов и расходов
        var incomeBatch = new List<TransactionDto>(batchSize);
        var expenseBatch = new List<TransactionDto>(batchSize);
            
        // Счетчики и ошибки
        var errors = new List<string>();
        var totalCount = 0;
        var incomeCount = 0;
        var expenseCount = 0;
        var processedCount = 0;
        
        // Потоковая обработка транзакций
        foreach (var t in _parser.ParseOfxFile(command.FileStream))
        {
            totalCount++;
        
            // Валидация транзакции через отдельный валидатор
            var validation = OfxTransactionValidator.Validate(t);
            if (!validation.IsValid)
            {
                foreach (var err in validation.Errors)
                {
                    _logger.LogWarning(err);
                    errors.Add(err);
                }
                continue;
            }
        
            // Классификация и добавление в соответствующий пакет
            switch (t.Amount)
            {
                case > 0:
                    var incomeDto = new TransactionDto
                    {
                        TranId = t.TranId,
                        TranDate = t.TranDate,
                        Category = t.Category,
                        Description = t.Description,
                        Amount = t.Amount.Value,
                        FileId = fileId
                    };
                    incomeBatch.Add(incomeDto);
                    incomeCount++;
                        
                    // Если пакет заполнен, сохраняем и очищаем
                    if (incomeBatch.Count >= batchSize)
                    {
                        await SaveIncomeBatchAsync(incomeBatch, cancellationToken);
                        processedCount += incomeBatch.Count;
                        incomeBatch.Clear();
                            
                        // Логируем прогресс
                        _logger.LogInformation("Обработано транзакций: {ProcessedCount}/{TotalCount}", 
                            processedCount, totalCount);
                    }
                    break;
                        
                case < 0:
                    var expenseDto = new TransactionDto
                    {
                        TranId = t.TranId,
                        TranDate = t.TranDate,
                        Category = t.Category,
                        Description = t.Description,
                        Amount = -t.Amount.Value,
                        FileId = fileId
                    };
                    expenseBatch.Add(expenseDto);
                    expenseCount++;
                        
                    // Если пакет заполнен, сохраняем и очищаем
                    if (expenseBatch.Count >= batchSize)
                    {
                        await SaveExpenseBatchAsync(expenseBatch, cancellationToken);
                        processedCount += expenseBatch.Count;
                        expenseBatch.Clear();
                            
                        // Логируем прогресс
                        _logger.LogInformation("Обработано транзакций: {ProcessedCount}/{TotalCount}", 
                            processedCount, totalCount);
                    }
                    break;
            }
        }
        
        // Сохраняем оставшиеся транзакции
        if (incomeBatch.Count > 0)
        {
            await SaveIncomeBatchAsync(incomeBatch, cancellationToken);
            processedCount += incomeBatch.Count;
        }
            
        if (expenseBatch.Count > 0)
        {
            await SaveExpenseBatchAsync(expenseBatch, cancellationToken);
            processedCount += expenseBatch.Count;
        }
        
        // Обновляем статус файла на "Обработан"
        importedFile.Status = ImportedFileStatus.Processed;
        await _fileRepository.UpdateAsync(importedFile, cancellationToken);
        
        // Логируем итоговую информацию
        _logger.LogInformation("Импорт завершен. Всего транзакций: {TotalCount}", totalCount);
        _logger.LogInformation("Поступлений: {IncomeCount}, Трат: {ExpenseCount}", incomeCount, expenseCount);
            
        if (errors.Count > 0)
        {
            _logger.LogWarning("Ошибки при обработке файла: {Errors}", string.Join("; ", errors));
        }

        #endregion
    }

    private async Task<ImportedFile> CreateFile(ImportOfxFileCommand command, CancellationToken cancellationToken)
    {
        var importedFile = await _fileRepository.GetByFileNameAsync(command.FileName);
        if (importedFile != null)
        {
            throw new InvalidOperationException($"Файл с именем '{command.FileName}' уже импортирован и не может быть повторно загружен.");
        }
        importedFile = new ImportedFile(command.FileName);
        var validator = new ImportedFileValidator();
        var validationResult = validator.Validate(importedFile);
        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);
        importedFile.ImportedAt = DateTime.UtcNow;
        importedFile.Status = ImportedFileStatus.Processing;
        return await _fileRepository.CreateAsync(importedFile, cancellationToken);
    }

    /// <summary>
    /// Сохраняет пакет доходных транзакций
    /// </summary>
    private async Task SaveIncomeBatchAsync(List<TransactionDto> batch, CancellationToken cancellationToken)
    {
        try
        {
            var categoryCache = new Dictionary<string, Category?>();
            var entities = new List<BankTransaction>(batch.Count);
            foreach (var dto in batch)
            {
                if (string.IsNullOrWhiteSpace(dto.Category))
                    throw new Exception($"Не указана категория у транзакции {dto.TranId}");
                if (!categoryCache.TryGetValue(dto.Category, out var category) || category == null)
                {
                    category = await _categoryRepository.GetOrCreateAsync(dto.Category, cancellationToken);
                    categoryCache[dto.Category] = category;
                }
                var entity = new BankTransaction(
                    fitId: dto.TranId ?? string.Empty,
                    date: dto.TranDate ?? DateTime.MinValue,
                    amount: dto.Amount,
                    description: dto.Description ?? string.Empty,
                    categoryId: category.Id
                );
                entities.Add(entity);
            }
            foreach (var entity in entities)
            {
                await _transactionRepository.CreateAsync(entity, cancellationToken);
            }
            _logger.LogInformation("Сохранен пакет доходных транзакций: {Count}", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении пакета доходных транзакций");
            throw;
        }
    }

    /// <summary>
    /// Сохраняет пакет расходных транзакций
    /// </summary>
    private async Task SaveExpenseBatchAsync(List<TransactionDto> batch, CancellationToken cancellationToken)
    {
        try
        {
            var categoryCache = new Dictionary<string, Category?>();
            var entities = new List<BankTransaction>(batch.Count);
            foreach (var dto in batch)
            {
                if (string.IsNullOrWhiteSpace(dto.Category))
                    throw new Exception($"Не указана категория у транзакции {dto.TranId}");
                if (!categoryCache.TryGetValue(dto.Category, out var category) || category == null)
                {
                    category = await _categoryRepository.GetOrCreateAsync(dto.Category, cancellationToken);
                    categoryCache[dto.Category] = category;
                }
                var entity = new BankTransaction(
                    fitId: dto.TranId ?? string.Empty,
                    date: dto.TranDate ?? DateTime.MinValue,
                    amount: dto.Amount,
                    description: dto.Description ?? string.Empty,
                    categoryId: category.Id
                );
                entities.Add(entity);
            }
            foreach (var entity in entities)
            {
                await _transactionRepository.CreateAsync(entity, cancellationToken);
            }
            _logger.LogInformation("Сохранен пакет расходных транзакций: {Count}", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении пакета расходных транзакций");
            throw;
        }
    }
}
