using HomeFinancial.Application.Common;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Domain.Entities;
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
    private readonly ILogger _logger;
    private readonly ImportSettings _importSettings;
    private readonly IImportSessionFactory _importSessionFactory;
    private readonly IStatementProcessor _statementProcessor;

    /// <summary>
    /// Создает новый экземпляр обработчика импорта OFX-файла
    /// </summary>
    public ImportOfxFileHandler(
        IOfxParser parser,
        ILogger<ImportOfxFileHandler> logger,
        IOptions<ImportSettings> importSettings,
        IImportSessionFactory importSessionFactory,
        IStatementProcessor statementProcessor)
    {
        _parser = parser;
        _logger = logger;
        _importSettings = importSettings.Value;
        _importSessionFactory = importSessionFactory;
        _statementProcessor = statementProcessor;
    }
    
    /// <inheritdoc />
    public async Task<ApiResponse<ImportOfxFileResult>> HandleAsync(ImportOfxFileCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Импорт OFX-файла: {FileName}", command.FileName);

        await using var session = await _importSessionFactory.StartAsync(command.FileName, ct);
        var context = new ImportContext(session.File.Id, _importSettings.BatchSize);
            
        var statements = _parser.ParseStatementsAsync(command.FileStream, ct);

        await foreach (var statementDto in statements)
        {
            await _statementProcessor.ProcessStatementAsync(statementDto, context, ct);
        }
            
        // Вставляем оставшиеся транзакции
        await _statementProcessor.FlushBatchAsync(context, ct);
            
        // Завершаем импорт
        await session.CompleteAsync(ct);

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
}
