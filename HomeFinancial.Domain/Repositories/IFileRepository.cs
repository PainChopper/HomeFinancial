using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с импортированными файлами
/// </summary>
public interface IFileRepository : IGenericRepository<ImportedFile>
{
    /// <summary>
    /// Проверяет, существует ли файл с указанным именем
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <returns>true, если файл существует; иначе false</returns>
    Task<bool> ExistsByFileNameAsync(string fileName);
    
    /// <summary>
    /// Получает файл по имени
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <returns>Файл или null, если файл не найден</returns>
    Task<ImportedFile?> GetByFileNameAsync(string fileName);
}
