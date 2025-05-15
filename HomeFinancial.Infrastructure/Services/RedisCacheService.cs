using HomeFinancial.Application.Common;
using StackExchange.Redis;

namespace HomeFinancial.Infrastructure.Services;

/// <summary>
/// Реализация ICacheService через Redis (IDistributedCache).
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        ArgumentNullException.ThrowIfNull(redis, nameof(redis));
        _db = redis.GetDatabase();
    }

    /// <inheritdoc />
    public async Task<T?> HashGetAsync<T>(string key, string field)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(field, nameof(field));

        var value = await _db.HashGetAsync(key, field).ConfigureAwait(false);
        if (!value.HasValue)
            return default;

        // Поддержка Nullable<T>
        var stringValue = value.ToString();
        var targetType = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        var converted = underlyingType != null
            ? Convert.ChangeType(stringValue, underlyingType)
            : Convert.ChangeType(stringValue, targetType);
        return (T?)converted;
    }

    /// <inheritdoc />
    public Task HashSetAsync<T>(string key, string field, T value)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(field, nameof(field));

        return _db.HashSetAsync(key, field, value?.ToString());
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        return _db.StringSetAsync(key, value?.ToString(), expiry);
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        var value = await _db.StringGetAsync(key).ConfigureAwait(false);
        if (!value.HasValue)
            return default;

        // Поддержка Nullable<T>
        var stringValue = value.ToString();
        var tType = typeof(T);
        var underlying = Nullable.GetUnderlyingType(tType);
        var conv = underlying != null
            ? Convert.ChangeType(stringValue, underlying)
            : Convert.ChangeType(stringValue, tType);
        return (T?)conv;
    }
}
