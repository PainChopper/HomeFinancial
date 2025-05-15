using HomeFinancial.Domain.Common;

namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Базовый интерфейс репозитория для доменных сущностей
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
public interface IGenericRepository<T> where T : IEntity
{
    /// <summary>
    /// Получает все сущности
    /// </summary>
    /// <param name="ct"></param>
    /// <returns>Список сущностей</returns>
    Task<List<T>> GetAllAsync(CancellationToken ct  = default);

    /// <summary>
    /// Получает сущность по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    /// <param name="ct"></param>
    /// <returns>Сущность или null, если сущность не найдена</returns>
    Task<T?> GetByIdAsync(int id, CancellationToken ct  = default);

    /// <summary>
    /// Создает новую сущность
    /// </summary>
    /// <param name="entity">Сущность для создания</param>
    /// <param name="ct"></param>
    /// <returns>Созданная сущность</returns>
    Task<T> CreateAsync(T entity, CancellationToken ct  = default);

    /// <summary>
    /// Обновляет существующую сущность
    /// </summary>
    /// <param name="entity">Сущность для обновления</param>
    /// <param name="ct"></param>
    Task UpdateAsync(T entity, CancellationToken ct  = default);

    /// <summary>
    /// Удаляет сущность по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    /// <param name="ct"></param>
    Task DeleteAsync(int id, CancellationToken ct  = default);
}
