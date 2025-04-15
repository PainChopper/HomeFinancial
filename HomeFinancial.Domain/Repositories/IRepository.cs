using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Базовый интерфейс репозитория для доменных сущностей
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
public interface IRepository<T> where T : IEntity
{
    /// <summary>
    /// Получает все сущности
    /// </summary>
    /// <returns>Список сущностей</returns>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// Получает сущность по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    /// <returns>Сущность или null, если сущность не найдена</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Создает новую сущность
    /// </summary>
    /// <param name="entity">Сущность для создания</param>
    /// <returns>Созданная сущность</returns>
    Task<T> CreateAsync(T entity);

    /// <summary>
    /// Обновляет существующую сущность
    /// </summary>
    /// <param name="entity">Сущность для обновления</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Удаляет сущность по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    Task DeleteAsync(int id);
}
