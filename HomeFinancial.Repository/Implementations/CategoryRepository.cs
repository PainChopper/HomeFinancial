using HomeFinancial.Data;
using HomeFinancial.Data.Models;
using HomeFinancial.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Repository.Implementations;

/// <summary>
/// Репозиторий для работы с категориями
/// </summary>
public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(HomeFinancialDbContext dbContext, ILogger<GenericRepository<Category>> logger)
        : base(dbContext, logger)
    {
    }

    /// <summary>
    /// Получает категорию по имени
    /// </summary>
    /// <param name="name">Имя категории</param>
    /// <returns>Категория или null, если категория не найдена</returns>
    private async Task<Category?> GetByNameAsync(string name)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.Name == name);
    }

    /// <summary>
    /// Получает существующую категорию по имени или создает новую
    /// </summary>
    /// <param name="name">Имя категории</param>
    /// <returns>Существующая или новая категория</returns>
    public async Task<Category> GetOrCreateAsync(string name)
    {
        var category = await GetByNameAsync(name);
        if (category != null)
        {
            return category;
        }

        category = new Category { Name = name };
        return await CreateAsync(category);
    }

    /// <summary>
    /// Создает новую категорию
    /// </summary>
    /// <param name="category">Категория для создания</param>
    /// <returns>Созданная категория</returns>
    /// <exception cref="InvalidOperationException">Если категория с таким именем уже существует</exception>
    public override async Task<Category> CreateAsync(Category category)
    {
        Logger.LogInformation("Attempting to create category: {CategoryName}", category.Name);

        var exist = await GetByNameAsync(category.Name);
        if (exist == null)
        {
            return await base.CreateAsync(category);
        }
        Logger.LogWarning("Category '{ExistingCategory}' already exists.", exist.Name);
        throw new InvalidOperationException($"Category '{exist.Name}' already exists.");
    }
}
