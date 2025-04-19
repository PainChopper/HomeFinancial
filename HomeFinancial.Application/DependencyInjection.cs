using Microsoft.Extensions.DependencyInjection;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Application.Services;
using HomeFinancial.OfxParser;

namespace HomeFinancial.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IFileImportService, FileImportService>();
        services.AddScoped<IOfxParser, OfxParser.OfxParser>();
        return services;
    }
}
