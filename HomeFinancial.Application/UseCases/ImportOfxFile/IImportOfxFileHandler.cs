namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Интерфейс обработчика импорта OFX-файла
/// </summary>
public interface IImportOfxFileHandler
{
    Task HandleAsync(ImportOfxFileCommand command, CancellationToken cancellationToken = default);
}
