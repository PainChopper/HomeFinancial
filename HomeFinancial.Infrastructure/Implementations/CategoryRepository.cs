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
    : GenericRepository<TransactionCategory>(dbContext, logger), ICategoryRepository
{
    //inheritdoc
    public async Task<int> GetOrCreateCategoryIdAsync(string categoryName)
    {
        return await RetryPolicyHelper.RetryAsync(async () =>
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
    LIMIT 1;
    ";

            var connection = DbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;

            var param = command.CreateParameter();
            param.ParameterName = "@CategoryName";
            param.Value = categoryName;
            command.Parameters.Add(param);
            return (int) (await command.ExecuteScalarAsync())!;
        });
    }
}
