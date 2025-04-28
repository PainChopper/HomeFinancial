using HomeFinancial.Application.Common;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.Implementations;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HomeFinancial.Infrastructure.Utils;
using StackExchange.Redis;
using HomeFinancial.Infrastructure.HostedServices;
using HomeFinancial.Infrastructure.Services;

namespace HomeFinancial.Infrastructure;

/// <summary>
/// Методы расширения для регистрации инфраструктурных сервисов (DbContext, репозитории и др.)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        
        var postgresConnectionString = configuration.GetConnectionString("PostgresConnection")
                                       ?? throw new ArgumentNullException(nameof(configuration), "Не найдена строка подключения Postgres");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(postgresConnectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSnakeCaseNamingConvention());

        var redisConnectionString = configuration.GetConnectionString("Redis")
                                    ?? throw new ArgumentNullException(nameof(configuration), "Не найдена строка подключения Redis");
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<ILeaseService, RedisLeaseService>();
        
        // Регистрация репозиториев
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<ITransactionInserter, TransactionInserter>();
        services.AddSingleton<RetryPolicyHelper>();
       
        // Hosted service для прогрева кэша категорий
        services.AddHostedService<CategoryCacheWarmupService>();

        return services;
    }
}
