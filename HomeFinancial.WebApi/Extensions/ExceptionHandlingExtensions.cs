using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinancial.WebApi.Extensions;

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
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    logger.LogError(contextFeature.Error, "Произошла необработанная ошибка.");
                    var problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Внутренняя ошибка сервера.",
                        Detail = contextFeature.Error.Message,
                        Instance = context.Request.Path
                    };
                    await context.Response.WriteAsJsonAsync(problem);
                }
            });
        });
        return app;
    }
}
