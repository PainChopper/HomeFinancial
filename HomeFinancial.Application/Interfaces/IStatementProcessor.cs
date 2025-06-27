using HomeFinancial.Application.UseCases.ImportOfxFile;
using HomeFinancial.OfxParser.Dto;

namespace HomeFinancial.Application.Interfaces;

/// <summary>
/// Интерфейс процессора обработки банковских выписок
/// </summary>
public interface IStatementProcessor
{
    /// <summary>
    /// Обрабатывает выписку и добавляет транзакции в контекст импорта
    /// </summary>
    /// <param name="statementDto">DTO выписки</param>
    /// <param name="context">Контекст импорта</param>
    /// <param name="ct">Токен отмены</param>
    Task ProcessStatementAsync(
        OfxAccountStatementDto statementDto,
        ImportContext context,
        CancellationToken ct);

    /// <summary>
    /// Сохраняет все оставшиеся транзакции в пакете независимо от его текущего размера
    /// </summary>
    /// <param name="context">Контекст импорта</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Результат вставки пакета</returns>
    Task<BulkInsertResult> FlushBatchAsync(ImportContext context, CancellationToken ct);
}
