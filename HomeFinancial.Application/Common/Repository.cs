namespace HomeFinancial.Application.Common;

/// <summary>
/// Интерфейс для реализации репозитория в инфраструктурном слое
/// </summary>
public interface IRepositoryImplementation<TEntity> : IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Асинхронно добавляет коллекцию сущностей
    /// </summary>
    new Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
}