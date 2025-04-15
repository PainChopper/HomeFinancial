using HomeFinancial.Data;
using HomeFinancial.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Repository.Implementations;

/// <summary>
/// Базовая реализация общего репозитория
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly HomeFinancialDbContext _dbContext;
    protected readonly ILogger<GenericRepository<T>> Logger;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(HomeFinancialDbContext dbContext, ILogger<GenericRepository<T>> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
