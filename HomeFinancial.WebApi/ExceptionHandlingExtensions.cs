using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.WebApi;

/// <summary>
/// Методы расширения для глобальной обработки ошибок
/// </summary>
public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Включает глобальный обработчик ошибок с логированием и JSON-ответом
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        var logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    logger.LogError(contextFeature.Error, "Произошла необработанная ошибка.");
                    var errorResponse = new
                    {
                        context.Response.StatusCode,
                        Message = "Внутренняя ошибка сервера."
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                }
            });
        });
        return app;
    }
}
