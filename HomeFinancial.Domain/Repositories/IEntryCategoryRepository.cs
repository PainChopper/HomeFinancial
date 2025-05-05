using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с категориями
/// </summary>
public interface IEntryCategoryRepository : IGenericRepository<EntryCategory>
{
    /// <summary>
    /// Получает идентификатор категории по имени. Если категории с указанным именем нет, создаёт новую.
    /// </summary>
    /// <param name="categoryName">Название категории</param>
    /// <returns>Идентификатор существующей или только что созданной категории</returns>
    Task<int> GetOrCreateCategoryIdAsync(string categoryName);
}
