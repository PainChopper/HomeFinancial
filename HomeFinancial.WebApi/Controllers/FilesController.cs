using HomeFinancial.Application.UseCases.ImportOfxFile;
using HomeFinancial.Application.Common;
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
    /// <response code="200">Файл успешно импортирован</response>
    /// <response code="400">Ошибка валидации или бизнес-логики</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [ProducesResponseType(typeof(ApiResponse<ImportOfxFileResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ImportOfxFileResult>), 400)]
    [ProducesResponseType(typeof(ApiResponse<ImportOfxFileResult>), 500)]
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ImportOfxFileResult>>> ImportFile([FromForm] ImportFileForm form, CancellationToken cancellationToken)
    {
        await using var stream = form.File.OpenReadStream();
        var command = new ImportOfxFileCommand(form.FileName, stream);
        try
        {
            var response = await _importHandler.HandleAsync(command, cancellationToken);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            // В случае исключения возвращаем единый формат
            var error = new ApiResponse<ImportOfxFileResult>(false, null, $"Ошибка при импорте файла: {ex.Message}");
            return StatusCode(500, error);
        }
    }
}
