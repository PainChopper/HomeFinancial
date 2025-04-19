using HomeFinancial.Application.Interfaces;

namespace HomeFinancial.Application.Services;

/// <summary>
/// Заглушка сервиса импорта файлов.
/// </summary>
public class FileImportService : IFileImportService
{
    public async Task ImportAsync(string fileName, Stream fileStream, CancellationToken cancellationToken = default)
    {
        // Здесь будет логика сохранения файла/парсинга/валидации и т.д.
        // Пока просто читаем поток (пример заглушки)
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms, cancellationToken);
        // Можно добавить логирование или другую логику
    }
}
