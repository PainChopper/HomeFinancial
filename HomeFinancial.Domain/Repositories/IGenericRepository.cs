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
    /// <param name="cancellationToken"></param>
    /// <returns>Список сущностей</returns>
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken  = default);

    /// <summary>
    /// Получает сущность по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Сущность или null, если сущность не найдена</returns>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken  = default);

    /// <summary>
    /// Создает новую сущность
    /// </summary>
    /// <param name="entity">Сущность для создания</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Созданная сущность</returns>
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken  = default);

    /// <summary>
    /// Обновляет существующую сущность
    /// </summary>
    /// <param name="entity">Сущность для обновления</param>
    /// <param name="cancellationToken"></param>
    Task UpdateAsync(T entity, CancellationToken cancellationToken  = default);

    /// <summary>
    /// Удаляет сущность по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    /// <param name="cancellationToken"></param>
    Task DeleteAsync(int id, CancellationToken cancellationToken  = default);
}
