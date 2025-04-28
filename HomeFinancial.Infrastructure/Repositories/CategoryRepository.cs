using System.Data;
using HomeFinancial.Application.Common;
using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.Persistence;
using HomeFinancial.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Infrastructure.Repositories;

// Репозиторий для работы с категориями, кэшируем их в одном Redis-хэше
public sealed class CategoryRepository : GenericRepository<TransactionCategory>, ICategoryRepository
{
    private const string CategoriesHashKey = "Categories";
    private readonly RetryPolicyHelper _retryPolicyHelper;
    private readonly ICacheService      _cacheService;

    public CategoryRepository(ApplicationDbContext dbContext,
                              ILogger<CategoryRepository> logger,
                              RetryPolicyHelper retryPolicyHelper,
                              ICacheService cacheService)
        : base(dbContext)
    {
        _retryPolicyHelper = retryPolicyHelper ?? throw new ArgumentNullException(nameof(retryPolicyHelper));
        _cacheService      = cacheService      ?? throw new ArgumentNullException(nameof(cacheService));
    }

    /// <summary>
    /// Получает или создаёт ID категории по имени,
    /// кэшируя результат в Redis-хэше "Categories".
    /// </summary>
    public async Task<int> GetOrCreateCategoryIdAsync(string categoryName)
    {
        ArgumentNullException.ThrowIfNull(categoryName, nameof(categoryName));

        // Попытка взять из кэша
        var cached = await _cacheService.HashGetAsync<int?>(CategoriesHashKey, categoryName);
        if (cached.HasValue)
            return cached.Value;

        // Вставка или получение из БД
        var id = await _retryPolicyHelper.RetryAsync(async () =>
        {
            const string sql = @"
WITH ins AS (
    INSERT INTO transaction_categories (name)
    VALUES (@CategoryName)
    ON CONFLICT (name) DO NOTHING
    RETURNING id
)
SELECT id FROM ins
UNION ALL
SELECT id FROM transaction_categories WHERE name = @CategoryName
LIMIT 1;";

            var conn = DbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var p = cmd.CreateParameter();
            p.ParameterName = "@CategoryName";
            p.Value         = categoryName;
            cmd.Parameters.Add(p);

            var result = await cmd.ExecuteScalarAsync();
            return result is int v
                ? v
                : throw new InvalidOperationException($"Не удалось получить или создать категорию '{categoryName}'.");
        });

        // Записываем в кэш
        await _cacheService.HashSetAsync(CategoriesHashKey, categoryName, id);
        return id;
    }
}
