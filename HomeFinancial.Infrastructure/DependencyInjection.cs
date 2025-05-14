using HomeFinancial.Application.Common;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.HostedServices;
using HomeFinancial.Infrastructure.Persistence;
using HomeFinancial.Infrastructure.Repositories;
using HomeFinancial.Infrastructure.Services;
using HomeFinancial.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace HomeFinancial.Infrastructure;

/// <summary>
/// Методы расширения для регистрации инфраструктурных сервисов (DbContext, репозитории и др.)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        
        var connectionStrings = configuration
            .GetSection("ConnectionStrings")
            .Get<ConnectionStrings>() ?? throw new InvalidOperationException("Секция ConnectionStrings не найдена в конфигурации.");
        
        services.AddSingleton(connectionStrings);
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionStrings.Postgres)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSnakeCaseNamingConvention());

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionStrings.Redis));

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();        
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<ILeaseService, RedisLeaseService>();
        services.AddSingleton<RetryPolicyHelper>();
                
        // Регистрация репозиториев
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IEntryCategoryRepository, EntryCategoryRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<ITransactionInserter, TransactionInserter>();

       
        // Hosted service для прогрева кэша категорий
        services.AddHostedService<EntryCategoryCacheWarmupService>();

        return services;
    }
}
