using HomeFinancial.Application.Common;
using HomeFinancial.Application.UseCases.ImportOfxFile;
using HomeFinancial.OfxParser;
using HomeFinancial.OfxParser.Dto;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using HomeFinancial.Application.Interfaces;

namespace HomeFinancial.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IOfxParser, OfxParser.OfxParser>();
        services.AddScoped<IImportOfxFileHandler, ImportOfxFileHandler>();
        services.AddOptions<ImportSettings>()
            .BindConfiguration("ImportSettings");
        services.AddSingleton<IValidator<OfxTransactionDto>, OfxTransactionValidator>();
        services.AddScoped<IImportFileService, ImportFileService>();

        return services;
    }
}
