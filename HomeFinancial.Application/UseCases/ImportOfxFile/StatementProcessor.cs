using FluentValidation;
using HomeFinancial.Application.Common;
using HomeFinancial.Application.Dtos;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.OfxParser.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Реализация процессора банковских выписок
/// </summary>
public class StatementProcessor : IStatementProcessor
{
    private readonly ILogger _logger;
    private readonly IValidator<OfxTransactionDto> _transactionValidator;
    private readonly ITransactionInserter _transactionInserter;
    private readonly IBankRepository _bankRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IEntryCategoryRepository _entryCategoryRepository;
    private readonly ImportSettings _importSettings;

    /// <summary>
    /// Создает новый экземпляр процессора банковских выписок
    /// </summary>
    public StatementProcessor(
        ILogger<StatementProcessor> logger,
        IOptions<ImportSettings> importSettings,
        IValidator<OfxTransactionDto> transactionValidator,
        ITransactionInserter transactionInserter,
        IBankRepository bankRepository,
        IBankAccountRepository bankAccountRepository,
        IEntryCategoryRepository categoryRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _importSettings = importSettings.Value ?? throw new ArgumentNullException(nameof(importSettings));
        _transactionValidator = transactionValidator ?? throw new ArgumentNullException(nameof(transactionValidator));
        _transactionInserter = transactionInserter ?? throw new ArgumentNullException(nameof(transactionInserter));
        _bankRepository = bankRepository ?? throw new ArgumentNullException(nameof(bankRepository));
        _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
        _entryCategoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }

    /// <inheritdoc />
    public async Task ProcessStatementAsync(
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
            var dto = await CreateTransactionAsync(context.FileId, tran, account, ct);
            
            if (dto != null)
            {
                context.Batch.Add(dto);
                
                // Если пакет заполнен - отправляем на вставку
                if (context.Batch.Count >= _importSettings.BatchSize)
                {
                    await FlushBatchAsync(context, ct);
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
    /// <param name="fileId"></param>
    /// <param name="transaction">Транзакция для обработки</param>
    /// <param name="account">Банковский счёт</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>DTO транзакции для вставки, null если транзакция не прошла валидацию</returns>
    private async Task<TransactionInsertDto?> CreateTransactionAsync(int fileId, OfxTransactionDto transaction,
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
            FileId: fileId,
            FitId: transaction.Id,
            Date: transaction.TranDate,
            Amount: transaction.Amount,
            Description: transaction.Description,
            CategoryId: categoryId,
            BankAccountId: account.Id);
    }

    /// <inheritdoc />
    public async Task<BulkInsertResult> FlushBatchAsync(
        ImportContext context, 
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Batch.Count == 0)
        {
            return new BulkInsertResult(); // Нет транзакций для вставки
        }
        
        var bulkResult = await _transactionInserter.BulkInsertCopyAsync(context.Batch, ct);
        
        // Обновляем метрики импорта
        context.Inserted += bulkResult.Inserted;
        context.Duplicates += bulkResult.Duplicates;
        
        context.Batch.Clear();
        return bulkResult;
    }
}
