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

        try
        {
            // Получаем/создаём запись о файле и получаем его Id
            var importedFile = await _fileRepository.GetByFileNameAsync(command.FileName);
            if (importedFile == null)
            {
                importedFile = new Domain.Entities.ImportedFile(command.FileName)
                {
                    ImportedAt = DateTime.UtcNow,
                    Status = Domain.Entities.ImportedFileStatus.Processing
                };
                await _fileRepository.CreateAsync(importedFile, cancellationToken);
            }
            else
            {
                // Если файл уже существует, обновляем его статус
                importedFile.Status = Domain.Entities.ImportedFileStatus.Processing;
                await _fileRepository.UpdateAsync(importedFile, cancellationToken);
            }
            int fileId = importedFile.Id;

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
            importedFile.Status = Domain.Entities.ImportedFileStatus.Processed;
            await _fileRepository.UpdateAsync(importedFile, cancellationToken);

            // Логируем итоговую информацию
            _logger.LogInformation("Импорт завершен. Всего транзакций: {TotalCount}", totalCount);
            _logger.LogInformation("Поступлений: {IncomeCount}, Трат: {ExpenseCount}", incomeCount, expenseCount);
            
            if (errors.Count > 0)
            {
                _logger.LogWarning("Ошибки при обработке файла: {Errors}", string.Join("; ", errors));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при импорте файла {FileName}", command.FileName);
            
            // Обновляем статус файла на "Ошибка"
            var errorFile = await _fileRepository.GetByFileNameAsync(command.FileName);
            if (errorFile != null)
            {
                errorFile.Status = Domain.Entities.ImportedFileStatus.Error;
                await _fileRepository.UpdateAsync(errorFile, cancellationToken);
            }
            
            throw; // Пробрасываем исключение дальше
        }
    }

    /// <summary>
    /// Сохраняет пакет доходных транзакций
    /// </summary>
    private async Task SaveIncomeBatchAsync(List<TransactionDto> batch, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Реализовать маппинг из DTO в сущности и сохранение через репозиторий
            // var entities = batch.Select(dto => _mapper.Map<Income>(dto)).ToList();
            // await _incomeRepository.AddRangeAsync(entities, cancellationToken);
            
            _logger.LogInformation("Сохранен пакет доходных транзакций: {Count}", batch.Count);
            
            // Пока используем заглушку
            await Task.CompletedTask;
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
            // TODO: Реализовать маппинг из DTO в сущности и сохранение через репозиторий
            // var entities = batch.Select(dto => _mapper.Map<Expense>(dto)).ToList();
            // await _expenseRepository.AddRangeAsync(entities, cancellationToken);
            
            _logger.LogInformation("Сохранен пакет расходных транзакций: {Count}", batch.Count);
            
            // Пока используем заглушку
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении пакета расходных транзакций");
            throw;
        }
    }
}
