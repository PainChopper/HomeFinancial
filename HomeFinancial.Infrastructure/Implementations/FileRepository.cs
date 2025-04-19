using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Infrastructure.Implementations;

/// <summary>
/// Репозиторий для работы с импортированными файлами
/// </summary>
public class FileRepository(HomeFinancialDbContext dbContext, ILogger<GenericGenericRepository<ImportedFile>> logger)
    : GenericGenericRepository<ImportedFile>(dbContext, logger), IFileRepository
{
    /// <summary>
    /// Проверяет, существует ли файл с указанным именем в базе данных
    /// </summary>
    public async Task<bool> ExistsByFileNameAsync(string fileName)
        => await DbSet.AnyAsync(f => f.FileName == fileName);

    /// <summary>
    /// Получает файл по имени
    /// </summary>
    public async Task<ImportedFile?> GetByFileNameAsync(string fileName)
        => await DbSet.FirstOrDefaultAsync(f => f.FileName == fileName);

    /// <summary>
    /// Создает новую запись об импортированном файле
    /// </summary>
    public override async Task<ImportedFile> CreateAsync(ImportedFile file, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Attempting to add imported file: {FileName}", file.FileName);
        if (await ExistsByFileNameAsync(file.FileName))
        {
            Logger.LogWarning("File {FileName} is already recorded in the database.", file.FileName);
            throw new InvalidOperationException($"File {file.FileName} is already recorded in the database.");
        }
        file.ImportedAt = DateTime.UtcNow;
        return await base.CreateAsync(file, cancellationToken);
    }
}
