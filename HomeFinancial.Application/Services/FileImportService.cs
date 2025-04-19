using HomeFinancial.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Application.Services;

/// <summary>
/// Заглушка сервиса импорта файлов.
/// </summary>
public class FileImportService(ILogger<FileImportService> logger) : IFileImportService
{
    public async Task ImportAsync(string fileName, Stream fileStream, CancellationToken cancellationToken = default)
    {
        // Здесь будет логика сохранения файла/парсинга/валидации и т.д.
        // Пока просто читаем поток (пример заглушки)
        using var ms = new MemoryStream();
        try
        {
            await fileStream.CopyToAsync(ms, cancellationToken);
            // Можно добавить логирование или другую логику
            logger.LogInformation("Импорт файла завершён успешно: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при импорте файла: {ErrorMessage}", ex.Message);
            throw new InvalidOperationException("Файл уже был импортирован ранее.");
        }
    }
}
