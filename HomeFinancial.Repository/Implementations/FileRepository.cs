using HomeFinancial.Data;
using HomeFinancial.Data.Models;
using HomeFinancial.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeFinancial.Repository.Implementations;

/// <summary>
/// Репозиторий для работы с импортированными файлами
/// </summary>
public class FileRepository : GenericRepository<ImportedFile>, IFileRepository
{
    public FileRepository(HomeFinancialDbContext dbContext, ILogger<GenericRepository<ImportedFile>> logger)
        : base(dbContext, logger)
    {
    }

    /// <summary>
    /// Проверяет, существует ли файл с указанным именем в базе данных
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <returns>true, если файл существует; иначе false</returns>
    public async Task<bool> ExistsAsync(string fileName)
    {
        return await DbSet.AnyAsync(f => f.FileName == fileName);
    }

    /// <summary>
    /// Создает новую запись об импортированном файле
    /// </summary>
    /// <param name="file">Файл для создания</param>
    /// <returns>Созданная запись о файле</returns>
    /// <exception cref="InvalidOperationException">Если файл с таким именем уже существует</exception>
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
