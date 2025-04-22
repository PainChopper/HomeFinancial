using HomeFinancial.Domain.Entities;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeFinancial.Infrastructure.Implementations;

public abstract class BankTransactionRepositoryBase<T> where T : BankTransaction
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    protected BankTransactionRepositoryBase(ApplicationDbContext context, DbSet<T> dbSet)
    {
        _context = context;
        _dbSet = dbSet;
    }

    public virtual async Task AddAsync(T transaction, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsByFitIdAsync(string fitId)
    {
        return await _dbSet.AnyAsync(t => t.FitId == fitId);
    }

    public virtual async Task<T?> GetByFitIdAsync(string fitId)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.FitId == fitId);
    }

    public virtual async Task<List<T>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbSet.Where(t => t.CategoryId == categoryId).ToListAsync();
    }
}
