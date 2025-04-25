using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using LazyCache;

namespace HomeFinancial.Infrastructure.Implementations;

public class CategoryCache
{
    private readonly IAppCache _cache;
    private readonly ICategoryRepository _repo;
    private const string CacheKey = "all_categories";

    public CategoryCache(IAppCache cache, ICategoryRepository repo)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
    }

    public Task<List<Category>> GetAllAsync() =>
        _cache.GetOrAddAsync(CacheKey, () => _repo.GetAllAsync());

    public void Clear() => _cache.Remove(CacheKey);

    public async Task AddToCacheAsync(Category category)
    {
        var categories = await GetAllAsync();
        if (!categories.Any(c => c.Name == category.Name))
        {
            categories.Add(category);
            _cache.Add(CacheKey, categories);
        }
    }
}
