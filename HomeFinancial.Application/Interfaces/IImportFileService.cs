using HomeFinancial.Application.UseCases.ImportOfxFile;

namespace HomeFinancial.Application.Interfaces;

/// <summary>
/// Фабрика и менеджер сеансов импорта файла.
/// </summary>
public interface IImportFileService
{
    /// <summary>Начать импорт. Возвращает сессию с данными файла и lease.</summary>
    Task<ImportFileSession> StartAsync(string fileName, CancellationToken ct);

    /// <summary>Проверить и продлить lease.</summary>
    Task ValidateAndExtendAsync(ImportFileSession session);

    /// <summary>Завершить импорт: обновить статус файла и освободить lease.</summary>
    Task CompleteAsync(ImportFileSession session, CancellationToken ct);

    /// <summary>Освободить lease без изменения статуса (идемпотентно).</summary>
    Task ReleaseAsync(ImportFileSession session);
}
