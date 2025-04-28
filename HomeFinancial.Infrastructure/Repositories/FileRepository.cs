using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeFinancial.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с импортированными файлами
/// </summary>
public class FileRepository(ApplicationDbContext dbContext)
    : GenericRepository<BankFile>(dbContext), IFileRepository
{
    /// <summary>
    /// Возвращает файл с указанным именем из базы данных
    /// </summary>
    public async Task<BankFile?> GetByFileNameAsync(string fileName)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(f => f.FileName == fileName);
    }
}
