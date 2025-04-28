using HomeFinancial.Infrastructure.Persistence;
using HomeFinancial.Domain.Entities;
using HomeFinancial.Application.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeFinancial.Infrastructure.HostedServices;

/// <summary>
/// HostedService для прогрева кэша категорий в Redis.
/// </summary>
public class CategoryCacheWarmupService : IHostedService
{
    private const string CategoriesHashKey = "Categories";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CategoryCacheWarmupService> _logger;

    public CategoryCacheWarmupService(IServiceProvider serviceProvider, ILogger<CategoryCacheWarmupService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();

        var categories = await db.Set<TransactionCategory>()
            .AsNoTracking()
            .Select(c => new { c.Name, c.Id })
            .ToListAsync(cancellationToken);

        foreach (var cat in categories)
        {
            await cache.HashSetAsync(CategoriesHashKey, cat.Name, cat.Id);
            _logger.LogInformation("Прогрев кэша: категория {Name} -> {Id}", cat.Name, cat.Id);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
