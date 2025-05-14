using HomeFinancial.Application.Common;
using HomeFinancial.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Infrastructure.HostedServices;

/// <summary>
/// HostedService для прогрева кэша категорий в Redis.
/// </summary>
public class EntryCategoryCacheWarmupService : IHostedService
{
    private const string CategoriesHashKey = "Categories";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public EntryCategoryCacheWarmupService(IServiceProvider serviceProvider, ILogger<EntryCategoryCacheWarmupService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var repository = scope.ServiceProvider.GetRequiredService<IEntryCategoryRepository>();

        var categories = await repository.GetAllAsync(ct);

        foreach (var cat in categories)
        {
            await cache.HashSetAsync(CategoriesHashKey, cat.Name, cat.Id);
            _logger.LogInformation("Прогрев кэша: категория {Name} -> {Id}", cat.Name, cat.Id);
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
