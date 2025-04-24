using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Infrastructure.Implementations;

/// <summary>
/// Репозиторий для работы с категориями
/// </summary>
public class CategoryRepository(ApplicationDbContext dbContext, ILogger<GenericGenericRepository<Category>> logger)
    : GenericGenericRepository<Category>(dbContext, logger), ICategoryRepository
{
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
        category = new Category { Name = name };
        return await CreateAsync(category, cancellationToken);
    }

    /// <summary>
    /// Создает новую категорию
    /// </summary>
    public override async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Попытка создать категорию: {CategoryName}", category.Name);
        var exist = await GetByNameAsync(category.Name);
        if (exist == null)
            return await base.CreateAsync(category, cancellationToken);
        Logger.LogWarning("Категория '{ExistingCategory}' уже существует.", exist.Name);
        throw new InvalidOperationException($"Категория '{exist.Name}' уже существует.");
    }
}
