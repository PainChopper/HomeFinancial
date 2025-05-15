using FluentValidation;
using HomeFinancial.Application.Common;
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
    /// <param name="ct">Токен отмены операции</param>
    /// <returns>Результат импорта</returns>
    /// <response code="200">Файл успешно импортирован</response>
    /// <response code="400">Ошибка валидации или бизнес-логики</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [ProducesResponseType(typeof(ApiResponse<ImportOfxFileResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ImportOfxFileResult>), 400)]
    [ProducesResponseType(typeof(ApiResponse<ImportOfxFileResult>), 500)]
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ImportOfxFileResult>>> ImportFile([FromForm] ImportFileForm form, CancellationToken ct)
    {
        await using var stream = form.File.OpenReadStream();
        var command = new ImportOfxFileCommand(form.FileName, stream);
        try
        {
            var response = await _importHandler.HandleAsync(command, ct);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
        catch (ValidationException ex)
        {
            var error = new ApiResponse<ImportOfxFileResult>(false, null, ex.Message);
            return BadRequest(error);
        }
        catch (InvalidOperationException ex)
        {
            var error = new ApiResponse<ImportOfxFileResult>(false, null, ex.Message);
            return BadRequest(error);
        }
    }
}
