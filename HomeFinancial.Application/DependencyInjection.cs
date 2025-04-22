using HomeFinancial.Application.Common;
using HomeFinancial.Application.UseCases.ImportOfxFile;
using HomeFinancial.OfxParser;
using Microsoft.Extensions.DependencyInjection;

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
