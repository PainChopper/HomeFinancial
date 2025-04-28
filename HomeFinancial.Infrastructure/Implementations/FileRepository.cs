using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeFinancial.Infrastructure.Implementations;

/// <summary>
/// Репозиторий для работы с импортированными файлами
/// </summary>
public class FileRepository(ApplicationDbContext dbContext)
    : GenericRepository<BankFile>(dbContext), IFileRepository
{
    /// <summary>
    /// Проверяет, существует ли файл с указанным именем в базе данных
    /// </summary>
    public async Task<bool> ExistsByFileNameAsync(string fileName)
        => await DbSet.AnyAsync(f => f.FileName == fileName);
}
