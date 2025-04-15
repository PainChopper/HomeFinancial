using HomeFinancial.Data.Models;

namespace HomeFinancial.Repository.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с импортированными файлами
/// </summary>
public interface IFileRepository : IGenericRepository<ImportedFile>
{
    /// <summary>
    /// Проверяет, существует ли файл с указанным именем в базе данных
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <returns>true, если файл существует; иначе false</returns>
    Task<bool> ExistsAsync(string fileName);
}
