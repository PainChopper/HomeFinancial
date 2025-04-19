using HomeFinancial.Application.DTOs;
using HomeFinancial.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinancial.WebApi.Controllers;

/// <summary>
/// Контроллер для импорта банковских файлов и получения списка импортированных файлов с поддержкой cursor-based пагинации.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FilesController(IFileImportService fileImportService) : ControllerBase
{
    /// <summary>
    /// Импорт банковского файла (multipart/form-data)
    /// </summary>
    /// <param name="form">Форма импорта файла</param>
    /// <returns>Результат импорта</returns>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportFile([FromForm] ImportFileForm form)
    {
        await using var stream = form.File.OpenReadStream();
        await fileImportService.ImportAsync(form.FileName, stream);
        return Ok($"Файл '{form.FileName}' успешно импортирован");
    }

    /// <summary>
    /// Получить список импортированных файлов с поддержкой cursor-based пагинации
    /// </summary>
    /// <param name="cursor">Идентификатор последнего файла из предыдущей выборки (необязательный)</param>
    /// <param name="limit">Максимальное количество файлов в ответе (по умолчанию 20)</param>
    /// <returns>Список файлов и новый курсор</returns>
    [HttpGet("files")]
    public IActionResult GetFiles([FromQuery] int? cursor = null, [FromQuery] int limit = 20)
    {
        // TODO: Реализовать получение списка файлов через сервис Application слоя
        // Пример: var result = await _importService.GetFilesAsync(cursor, limit);
        // return Ok(result);
        return Ok(new { Files = new List<ImportedFileDto>(), NextCursor = (int?)null });
    }
}
