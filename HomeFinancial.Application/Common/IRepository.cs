namespace HomeFinancial.Application.Common;

/// <summary>
/// Универсальный репозиторий для работы с сущностями
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Асинхронно добавляет коллекцию сущностей
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
}
