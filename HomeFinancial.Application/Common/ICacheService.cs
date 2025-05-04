namespace HomeFinancial.Application.Common;

/// <summary>
/// Сервис для работы с кэшем (distributed cache).
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Получить значение поля хэша по ключу.
    /// </summary>
    Task<T?> HashGetAsync<T>(string key, string field);

    /// <summary>
    /// Установить значение поля хэша по ключу.
    /// </summary>
    Task HashSetAsync<T>(string key, string field, T value);

    /// <summary>
    /// Установить значение строкового ключа с опциональным временем жизни.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    /// <summary>
    /// Получить значение строкового ключа.
    /// </summary>
    Task<T?> GetAsync<T>(string key);
}