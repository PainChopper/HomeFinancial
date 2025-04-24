using HomeFinancial.Application.Common;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Интерфейс обработчика импорта OFX-файла
/// </summary>
public interface IImportOfxFileHandler
{
    Task<ApiResponse<ImportOfxFileResult>> HandleAsync(ImportOfxFileCommand command, CancellationToken cancellationToken = default);
}
