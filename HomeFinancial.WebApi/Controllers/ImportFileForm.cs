using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinancial.WebApi.Controllers;

/// <summary>
/// DTO для формы импорта банковского файла (используется для корректной работы Swagger)
/// </summary>
public class ImportFileForm
{
    [FromForm(Name = "file")]
    [Required(ErrorMessage = "Файл обязателен")]
    public required IFormFile File { get; set; }

    [FromForm(Name = "fileName")]
    [Required(ErrorMessage = "Имя файла не может быть пустым")]
    [MinLength(1, ErrorMessage = "Имя файла не может быть пустым")]
    public required string FileName { get; set; }
}