using System.Data.Common;
using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.Persistence;
using HomeFinancial.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Infrastructure.Implementations;

/// <summary>
/// Репозиторий для работы с категориями
/// </summary>
public class CategoryRepository(ApplicationDbContext dbContext, ILogger<CategoryRepository> logger)
    : GenericRepository<Category>(dbContext, logger), ICategoryRepository
{
    public async Task<int> GetOrCreateCategoryIdAsync(string categoryName)
    {
        await using var command = await CreateGetOrCreateCommand(categoryName);

        return (int) (await RetryPolicyHelper.RetryAsync(async () => await command.ExecuteScalarAsync()))!;
        
    }

    private async Task<DbCommand> CreateGetOrCreateCommand(string categoryName)
    {
        const string sql = @"
    WITH ins AS (
        INSERT INTO categories (category_name)
        VALUES (@CategoryName)
        ON CONFLICT (category_name) DO NOTHING
        RETURNING id
    )
    SELECT id FROM ins
    UNION ALL
    SELECT id FROM categories WHERE category_name = @CategoryName
    LIMIT 1;
    ";

        await using var connection = DbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = sql;

        var param = command.CreateParameter();
        param.ParameterName = "@CategoryName";
        param.Value = categoryName;
        command.Parameters.Add(param);
        return command;
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
        return await RetryPolicyHelper.RetryAsync(async () =>
        {
            var category = await GetByNameAsync(name);
            if (category != null)
                return category;
            var newCategory = new Category { Name = name };
            return await CreateAsync(newCategory, cancellationToken);
        });
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
