namespace HomeFinancial.WebApi.Extensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using HomeFinancial.Application.Common;

/// <summary>
/// Конфигурирует поведение API при некорректном состоянии модели.
/// </summary>
public static class ApiBehaviorExtensions
{
    /// <summary>
    /// Настраивает InvalidModelStateResponseFactory для возвращения ApiResponse с ошибками.
    /// </summary>
    public static IMvcBuilder ConfigureCustomApiBehavior(this IMvcBuilder builder)
    {
        return builder.ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var messages = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                var combined = string.Join(", ", messages);
                var apiResp = new ApiResponse<object>(false, null, combined);
                return new BadRequestObjectResult(apiResp);
            };
        });
    }
}