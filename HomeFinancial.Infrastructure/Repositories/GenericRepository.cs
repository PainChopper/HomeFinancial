using HomeFinancial.Domain.Common;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeFinancial.Infrastructure.Repositories;

/// <summary>
/// Базовая реализация общего репозитория
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
{
    protected readonly ApplicationDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DbSet = DbContext.Set<T>();
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await DbSet.ToListAsync(cancellationToken: cancellationToken);

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync([id], cancellationToken);

    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Add(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = Activator.CreateInstance<T>();
        typeof(T).GetProperty(nameof(IEntity.Id))?.SetValue(entity, id);
        DbSet.Remove(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
