using Microsoft.AspNetCore.Mvc;

namespace HomeFinancial.WebApi.Controllers
{
    /// <summary>
    /// DTO для формы импорта банковского файла (используется для корректной работы Swagger)
    /// </summary>
    public class ImportFileForm
    {
        [FromForm(Name = "file")]
        public required IFormFile File { get; init; }

        [FromForm(Name = "fileName")]
        public required string FileName { get; init; }
    }
}
