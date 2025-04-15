using HomeFinancial.Repository.Implementations;
using HomeFinancial.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFinancial.Repository;

/// <summary>
/// Методы расширения для регистрации репозиториев в DI-контейнере
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Регистрирует все репозитории в DI-контейнере
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <returns>Обновленная коллекция сервисов</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
        return services;
    }
}
