using HomeFinancial.Application.UseCases.ImportOfxFile;

namespace HomeFinancial.Application.Interfaces;

/// <summary>
/// Сервис для создания сессий импорта файлов
/// </summary>
public interface IFileImportSessionFactory
{
    /// <summary>Начать импорт. Возвращает сессию с данными файла и lease.</summary>
    Task<FileImportSession> StartAsync(string fileName, CancellationToken ct);
}
