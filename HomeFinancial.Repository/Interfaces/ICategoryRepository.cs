using HomeFinancial.Data.Models;

namespace HomeFinancial.Repository.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с категориями
/// </summary>
public interface ICategoryRepository : IGenericRepository<Category>
{
    /// <summary>
    /// Получает существующую категорию по имени или создает новую
    /// </summary>
    /// <param name="name">Имя категории</param>
    /// <returns>Существующая или новая категория</returns>
    Task<Category> GetOrCreateAsync(string name);
}
