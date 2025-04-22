using HomeFinancial.Application.Common.Abstractions;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.Database;
using HomeFinancial.Infrastructure.Implementations;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFinancial.Infrastructure;

/// <summary>
/// Методы расширения для регистрации инфраструктурных сервисов (DbContext, репозитории и др.)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSnakeCaseNamingConvention());
    
        services.AddScoped<IDatabaseHealthChecker, DatabaseHealthChecker>();
        
        // Регистрация репозиториев
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericGenericRepository<>));
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IFileRepository, FileRepository>();

        return services;
    }
}
