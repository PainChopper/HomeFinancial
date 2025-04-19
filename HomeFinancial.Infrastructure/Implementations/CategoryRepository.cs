using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Infrastructure.Implementations;

/// <summary>
/// Репозиторий для работы с категориями
/// </summary>
public class CategoryRepository : GenericGenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(HomeFinancialDbContext dbContext, ILogger<GenericGenericRepository<Category>> logger)
        : base(dbContext, logger)
    {
    }

    /// <summary>
    /// Получает категорию по имени
    /// </summary>
    public async Task<Category?> GetByNameAsync(string name)
        => await DbSet.FirstOrDefaultAsync(c => c.Name == name);

    /// <summary>
    /// Получает существующую категорию по имени или создает новую
    /// </summary>
    public async Task<Category> GetOrCreateAsync(string name, CancellationToken cancellationToken = default)
    {
        var category = await GetByNameAsync(name);
        if (category != null)
            return category;
        category = new Category(name);
        return await CreateAsync(category, cancellationToken);
    }

    /// <summary>
    /// Создает новую категорию
    /// </summary>
    public override async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Attempting to create category: {CategoryName}", category.Name);
        var exist = await GetByNameAsync(category.Name);
        if (exist == null)
            return await base.CreateAsync(category, cancellationToken);
        Logger.LogWarning("Category '{ExistingCategory}' already exists.", exist.Name);
        throw new InvalidOperationException($"Category '{exist.Name}' already exists.");
    }

    /// <summary>
    /// Удаляет категорию
    /// </summary>
    public async Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
