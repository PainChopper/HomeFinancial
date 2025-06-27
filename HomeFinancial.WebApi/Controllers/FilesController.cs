using FluentValidation;
using HomeFinancial.Application.Common;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Application.UseCases.ImportOfxFile;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinancial.WebApi.Controllers;

/// <summary>
/// Контроллер для работы с файлами
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
    /// Потоковый импорт банковского файла
    /// </summary>
    /// <param name="ct">Токен отмены операции</param>
    /// <returns>Результат импорта</returns>
    /// <response code="200">Файл успешно импортирован</response>
    /// <response code="400">Ошибка валидации или бизнес-логики</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [ProducesResponseType(typeof(ApiResponse<ImportOfxFileResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ImportOfxFileResult>), 400)]
    [ProducesResponseType(typeof(ApiResponse<ImportOfxFileResult>), 500)]
    [HttpPost]
    [Route("import")]
    [Consumes("application/octet-stream")]
    public async Task<ActionResult<ApiResponse<ImportOfxFileResult>>> ImportStream(CancellationToken ct)
    {
        // Получаем имя файла из заголовка
        if (!Request.Headers.TryGetValue("X-Filename", out var fileNameValues))
        {
            return BadRequest(new ApiResponse<ImportOfxFileResult>(false, null, "Требуется заголовок X-Filename"));
        }

        var fileName = fileNameValues.ToString();
        
        // Используем тело запроса напрямую как поток
        var command = new ImportOfxFileCommand(fileName, Request.Body);
        
        try
        {
            var response = await _importHandler.HandleAsync(command, ct);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ApiResponse<ImportOfxFileResult>(false, null, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage))));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<ImportOfxFileResult>(false, null, ex.Message));
        }
    }
}
