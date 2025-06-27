using HomeFinancial.Application.Common;
using HomeFinancial.Application.UseCases.ImportOfxFile;

namespace HomeFinancial.Application.Interfaces;

/// <summary>
/// Интерфейс обработчика импорта OFX-файла
/// </summary>
public interface IImportOfxFileHandler
{
    /// <summary>
    /// Обработчик импорта OFX-файла
    /// </summary>
    /// <param name="command">Команда на импорт OFX-файла</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Результат импорта OFX-файла</returns>
    Task<ApiResponse<ImportOfxFileResult>> HandleAsync(ImportOfxFileCommand command, CancellationToken ct = default);
}
