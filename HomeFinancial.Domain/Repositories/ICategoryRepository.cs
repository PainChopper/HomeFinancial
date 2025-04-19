using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с категориями
/// </summary>
public interface ICategoryRepository : IGenericRepository<Category>
{
    /// <summary>
    /// Получает категорию по имени
    /// </summary>
    /// <param name="name">Имя категории</param>
    /// <returns>Категория или null, если категория не найдена</returns>
    Task<Category?> GetByNameAsync(string name);

    /// <summary>
    /// Получает существующую категорию по имени или создает новую
    /// </summary>
    /// <param name="name">Имя категории</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Существующая или новая категория</returns>
    Task<Category> GetOrCreateAsync(string name, CancellationToken cancellationToken);

    Task DeleteAsync(Category category, CancellationToken cancellationToken);
}
