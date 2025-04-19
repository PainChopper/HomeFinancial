namespace HomeFinancial.Application.Interfaces;

/// <summary>
/// Сервис для импорта банковских файлов (Application-слой)
/// </summary>
public interface IFileImportService
{
    /// <summary>
    /// Импортировать файл.
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="fileStream">Поток файла</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task ImportAsync(string fileName, Stream fileStream, CancellationToken cancellationToken = default);
}
