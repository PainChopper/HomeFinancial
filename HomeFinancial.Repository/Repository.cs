// Repository.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HomeFinancial.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFinancial.Repository
{
    // DbContext
    public class HomeFinancialDbContext : DbContext
    {
        public HomeFinancialDbContext(DbContextOptions<HomeFinancialDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }
        public DbSet<ImportedFile> ImportedFiles { get; set; }
    }

    // Generic Repository Interface
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> CreateAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }

    // Generic Repository Implementation
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly HomeFinancialDbContext _dbContext;
        protected readonly ILogger<GenericRepository<T>> Logger;
        protected readonly DbSet<T> DbSet;

        public GenericRepository(HomeFinancialDbContext dbContext, ILogger<GenericRepository<T>> logger)
        {
            _dbContext = dbContext;
            Logger = logger;
            DbSet = _dbContext.Set<T>();
        }

        public virtual async Task<List<T>> GetAllAsync() => await DbSet.ToListAsync();

        public virtual async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);

        public virtual async Task<T> CreateAsync(T entity)
        {
            DbSet.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            DbSet.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                DbSet.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    // Specific Repositories Interfaces
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category> GetOrCreateAsync(string name);
    }

    public interface ITransactionRepository : IGenericRepository<BankTransaction>
    {
    }

    public interface IFileRepository : IGenericRepository<ImportedFile>
    {
        Task<bool> ExistsAsync(string fileName);
    }

    // Specific Repositories Implementations
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(HomeFinancialDbContext dbContext, ILogger<GenericRepository<Category>> logger)
            : base(dbContext, logger)
        {
        }

        private async Task<Category?> GetByNameAsync(string name)
        {
            return await DbSet.FirstOrDefaultAsync(c => c.Name == name);
        }

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

    public class TransactionRepository : GenericRepository<BankTransaction>, ITransactionRepository
    {
        public TransactionRepository(HomeFinancialDbContext dbContext, ILogger<GenericRepository<BankTransaction>> logger)
            : base(dbContext, logger)
        {
        }

        public override async Task<BankTransaction> CreateAsync(BankTransaction transaction)
        {
            Logger.LogInformation("Attempting to add transaction with FITID: {FitId}", transaction.FitId);
            if (!await DbSet.AnyAsync(x => x.FitId == transaction.FitId))
            {
                return await base.CreateAsync(transaction);
            }
            Logger.LogWarning("Transaction with FITID={FitId} already exists.", transaction.FitId);
            throw new InvalidOperationException($"FITID={transaction.FitId} already exists.");
        }
    }

    public class FileRepository : GenericRepository<ImportedFile>, IFileRepository
    {
        public FileRepository(HomeFinancialDbContext dbContext, ILogger<GenericRepository<ImportedFile>> logger)
            : base(dbContext, logger)
        {
        }

        public async Task<bool> ExistsAsync(string fileName)
        {
            return await DbSet.AnyAsync(f => f.FileName == fileName);
        }

        public override async Task<ImportedFile> CreateAsync(ImportedFile file)
        {
            Logger.LogInformation("Attempting to add imported file: {FileName}", file.FileName);
            if (await ExistsAsync(file.FileName))
            {
                Logger.LogWarning("File {FileName} is already recorded in the database.", file.FileName);
                throw new InvalidOperationException($"File {file.FileName} is already recorded in the database.");
            }

            file.ImportedAt = DateTime.UtcNow;
            return await base.CreateAsync(file);
        }
    }

    // Extension Method for Registering Repositories
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            return services;
        }
    }
}
