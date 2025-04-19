using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HomeFinancial.WebApi;

/// <summary>
/// Методы расширения для глобальной обработки ошибок
/// </summary>
public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Включает глобальный обработчик ошибок с логированием и JSON-ответом
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app, ILogger logger)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    logger.LogError(contextFeature.Error, "An unhandled exception occurred.");
                    var errorResponse = new
                    {
                        context.Response.StatusCode,
                        Message = "Internal Server Error."
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                }
            });
        });
        return app;
    }
}
