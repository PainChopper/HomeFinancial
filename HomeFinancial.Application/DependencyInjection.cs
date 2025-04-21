using HomeFinancial.Application.Common;
using Microsoft.Extensions.DependencyInjection;
using HomeFinancial.OfxParser;
using HomeFinancial.Application.UseCases.ImportOfxFile;

namespace HomeFinancial.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOfxParser, OfxParser.OfxParser>();
        services.AddScoped<IImportOfxFileHandler, ImportOfxFileHandler>();
        services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
        return services;
    }
}
