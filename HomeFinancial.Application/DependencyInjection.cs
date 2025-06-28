using HomeFinancial.Application.Common;
using HomeFinancial.Application.UseCases.ImportOfxFile;
using HomeFinancial.OfxParser;
using HomeFinancial.OfxParser.Dto;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using HomeFinancial.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HomeFinancial.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IOfxParser, OfxParser.OfxParser>();
        services.AddScoped<IImportOfxFileHandler, ImportOfxFileHandler>();
        
        services
            .Configure<ImportSettings>(configuration.GetSection("Import"));
            
        services.AddSingleton<IValidator<OfxTransactionDto>, OfxTransactionValidator>();
        services.AddSingleton<IImportSessionFactory, ImportSessionFactory>();
        services.AddScoped<IStatementProcessor, StatementProcessor>();

        return services;
    }
}
