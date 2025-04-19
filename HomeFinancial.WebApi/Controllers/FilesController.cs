using HomeFinancial.Application.UseCases.ImportOfxFile;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinancial.WebApi.Controllers;

/// <summary>
/// Контроллер для импорта банковских файлов и получения списка импортированных файлов с поддержкой cursor-based пагинации.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IImportOfxFileHandler _importHandler;

    public FilesController(IImportOfxFileHandler importHandler)
    {
        _importHandler = importHandler ?? throw new ArgumentNullException(nameof(importHandler));
    }

    /// <summary>
    /// Импорт банковского файла (multipart/form-data)
    /// </summary>
    /// <param name="form">Форма импорта файла</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат импорта</returns>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportFile([FromForm] ImportFileForm form, CancellationToken cancellationToken)
    {
        if (form.File == null)
        {
            return BadRequest("Файл не был выбран для загрузки.");
        }

        await using var stream = form.File.OpenReadStream();
        var command = new ImportOfxFileCommand(form.FileName, stream);
        try
        {
            await _importHandler.HandleAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return BadRequest($"Ошибка при импорте файла: {ex.Message}");
        }
        return Ok("Файл успешно импортирован.");
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
        // TODO: Реализовать получение списка файлов через Application-слой
        return Ok(new { Files = new List<object>(), NextCursor = (int?)null });
    }
}
