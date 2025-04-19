using Microsoft.AspNetCore.Mvc;

namespace HomeFinancial.WebApi.Controllers
{
    /// <summary>
    /// DTO для формы импорта банковского файла (используется для корректной работы Swagger)
    /// </summary>
    public class ImportFileForm
    {
        [FromForm(Name = "file")]
        public required IFormFile File { get; set; }

        [FromForm(Name = "fileName")]
        public required string FileName { get; set; }
    }
}
