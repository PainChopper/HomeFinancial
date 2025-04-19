using Microsoft.Extensions.DependencyInjection;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Application.Services;

namespace HomeFinancial.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IFileImportService, FileImportService>();
        return services;
    }
}
