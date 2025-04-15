using HomeFinancial.Domain.Common;
using HomeFinancial.Domain.Repositories;

namespace HomeFinancial.Repository.Interfaces;

/// <summary>
/// Общий интерфейс репозитория для базовых CRUD-операций
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
public interface IGenericRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
