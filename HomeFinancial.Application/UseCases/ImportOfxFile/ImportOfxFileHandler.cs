using FluentValidation;
using HomeFinancial.Application.Common;
using HomeFinancial.Application.Dtos;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.OfxParser;
using HomeFinancial.OfxParser.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Обработчик сценария импорта OFX-файла
/// </summary> 
public class ImportOfxFileHandler : IImportOfxFileHandler
{
    private readonly IOfxParser _parser;
    private readonly IEntryCategoryRepository _entryCategoryRepository;
    private readonly ILogger _logger;
    private readonly ImportSettings _importSettings;
    private readonly IValidator<OfxTransactionDto> _transactionValidator;
    private readonly ITransactionInserter _transactionInserter;
    private readonly IBankRepository _bankRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IImportFileService _importFileService;

    public ImportOfxFileHandler(
        IOfxParser parser,
        IEntryCategoryRepository entryCategoryRepository,
        ILogger<ImportOfxFileHandler> logger,
        IOptions<ImportSettings> importSettings,
        IValidator<OfxTransactionDto> transactionValidator,
        ITransactionInserter transactionInserter,
        IImportFileService importFileService,
        IBankRepository bankRepository,
        IBankAccountRepository bankAccountRepository)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _entryCategoryRepository = entryCategoryRepository ?? throw new ArgumentNullException(nameof(entryCategoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _importSettings = importSettings.Value ?? throw new ArgumentNullException(nameof(importSettings));
        _transactionValidator = transactionValidator ?? throw new ArgumentNullException(nameof(transactionValidator));
        _transactionInserter = transactionInserter ?? throw new ArgumentNullException(nameof(transactionInserter));
        _importFileService = importFileService ?? throw new ArgumentNullException(nameof(importFileService));
        _bankRepository = bankRepository ?? throw new ArgumentNullException(nameof(bankRepository));
        _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
    }
    
    /// <inheritdoc />
    public async Task<ApiResponse<ImportOfxFileResult>> HandleAsync(ImportOfxFileCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Импорт OFX-файла: {FileName}", command.FileName);

        var session = await _importFileService.StartAsync(command.FileName, ct);
        try
        {
            var context = new ImportContext(session, _importSettings.BatchSize);
            
            var statements = _parser.ParseStatementsAsync(command.FileStream, ct);

            await foreach (var statementDto in statements)
            {
                await ProcessStatementAsync(statementDto, context, ct);
            }
            
            // Вставляем оставшиеся транзакции
            if (context.Batch.Count > 0)
            {
                await _importFileService.ValidateAndExtendAsync(context.Session);
                var bulkResult = await _transactionInserter.BulkInsertCopyAsync(context.Batch, ct);
                context.Inserted += bulkResult.Inserted;
                context.Duplicates += bulkResult.Duplicates;
            }
            
            // Завершаем импорт
            await _importFileService.CompleteAsync(context.Session, ct);

            var result = new ImportOfxFileResult { 
                TotalCount = context.Total, 
                ImportedCount = context.Inserted, 
                ErrorCount = context.Errors, 
                SkippedDuplicateCount = context.Duplicates 
            };
            
            _logger.LogInformation("Импорт OFX-файла {FileName} завершён. Всего транзакций: {TotalCount}, успешно импортировано: {ImportedCount}", 
                command.FileName, result.TotalCount, result.ImportedCount);

            return new ApiResponse<ImportOfxFileResult>(true, result);
        }
        finally
        {
            await _importFileService.ReleaseAsync(session);
        }
    }

    private async Task ProcessStatementAsync(
        OfxAccountStatementDto statementDto,
        ImportContext context,
        CancellationToken ct)
    {
        // Получаем или создаём банк и счёт
        var bank = await _bankRepository.GetOrCreateAsync(
            statementDto.BankId,
            "Неизвестный банк",
            ct);

        var account = await _bankAccountRepository.GetOrCreateAsync(
            bank.Id,
            statementDto.BankAccountId,
            statementDto.BankAccountType,
            ct);

        // Обрабатываем транзакции
        await foreach (var tran in statementDto.Transactions.WithCancellation(ct))
        {
            var dto = await CreateTransactionAsync(
                tran,
                context.ImportedFile,
                account,
                ct);
            
            if (dto != null)
            {
                context.Batch.Add(dto);
                
                // Если пакет заполнен - отправляем на вставку
                if (context.Batch.Count >= _importSettings.BatchSize)
                {
                    var bulkResult = await SaveBatchAsync(context, ct);
                    context.Inserted += bulkResult.Inserted;
                    context.Duplicates += bulkResult.Duplicates;
                }
            }
            else
            {
                context.Errors++;
            }
        }
    }

    /// <summary>
    /// Обрабатывает отдельную транзакцию из statement-а 
    /// </summary>
    /// <param name="transaction">Транзакция для обработки</param>
    /// <param name="importedFile">Импортируемый файл</param>
    /// <param name="account">Банковский счёт</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>DTO транзакции для вставки, null если транзакция не прошла валидацию</returns>
    private async Task<TransactionInsertDto?> CreateTransactionAsync(
        OfxTransactionDto transaction,
        BankFile importedFile,
        BankAccount account,
        CancellationToken ct)
    {
        // Валидация транзакции через FluentValidation
        var validationResult = await _transactionValidator.ValidateAsync(transaction, ct);
        if (!validationResult.IsValid)
        {
            var errorList = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Транзакция [{Id}] пропущена: {Errors}", transaction.Id, errorList);
            return null;
        }

        // Определяем или создаём категорию и формируем DTO
        var categoryId = await _entryCategoryRepository.GetOrCreateCategoryIdAsync(transaction.Category);
        return new TransactionInsertDto(
            FileId: importedFile.Id,
            FitId: transaction.Id,
            Date: transaction.TranDate,
            Amount: transaction.Amount,
            Description: transaction.Description,
            CategoryId: categoryId,
            BankAccountId: account.Id);
    }

    /// <summary>
    /// Отправляет пакет транзакций на вставку в базу данных
    /// </summary>
    /// <param name="context">Контекст импорта</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Результат вставки</returns>
    private async Task<BulkInsertResult> SaveBatchAsync(
        ImportContext context,
        CancellationToken ct)
    {
        await _importFileService.ValidateAndExtendAsync(context.Session);
        var bulkResult = await _transactionInserter.BulkInsertCopyAsync(context.Batch, ct);
        context.Batch.Clear();
        return bulkResult;
    }
}
