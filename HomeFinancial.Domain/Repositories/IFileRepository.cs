using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с импортированными файлами
/// </summary>
public interface IFileRepository : IGenericRepository<BankFile>
{
    /// <summary>
    /// Получает файл по имени
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <returns>Файл, если найден; иначе null</returns>
    Task<BankFile?> GetByFileNameAsync(string fileName);
}
