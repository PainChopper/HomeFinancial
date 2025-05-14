using HomeFinancial.OfxParser.Dto;

namespace HomeFinancial.OfxParser;

/// <summary>
/// Интерфейс парсера OFX-файлов
/// </summary>
public interface IOfxParser
{
    /// <summary>
    /// Парсит все банковские выписки из OFX файла.
    /// </summary>
    /// <param name="stream">Поток с XML данными</param>
    /// <param name="ct">Токен отмены операции</param>
    /// <returns>Асинхронный перечислитель банковских выписок</returns>
    IAsyncEnumerable<OfxAccountStatementDto> ParseStatementsAsync(Stream stream, CancellationToken ct = default);
}