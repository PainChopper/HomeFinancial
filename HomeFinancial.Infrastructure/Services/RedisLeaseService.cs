using HomeFinancial.Application.Interfaces;
using StackExchange.Redis;

namespace HomeFinancial.Infrastructure.Services;

public sealed class RedisLeaseService : ILeaseService
{
    private readonly IDatabase _db;
    private const string LeaseKeyPrefix = "import:bankfile:lease";

    private static readonly LuaScript ExtendScript = LuaScript.Prepare(@"
        return (redis.call('get', @leaseKey) == @leaseId)
            and redis.call('pexpire', @leaseKey, @ttl)
            or 0
    ");

    private static readonly LuaScript ReleaseScript = LuaScript.Prepare(@"
        return (redis.call('get', @leaseKey) == @leaseId)
            and redis.call('del', @leaseKey)
            or 0
    ");

    private readonly LoadedLuaScript _releaseLoaded;

    public RedisLeaseService(IConnectionMultiplexer redis)
    {
        ArgumentNullException.ThrowIfNull(redis, nameof(redis));
        _db = redis.GetDatabase();
        
        // Загружаем скрипты на сервер
        var server = redis.GetServer(redis.GetEndPoints().First());
        ExtendScript.Load(server);
        _releaseLoaded = ReleaseScript.Load(server);
    }

    public async Task<Guid> AcquireLeaseAsync(string key, TimeSpan leaseTime)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

        var redisKey = (RedisKey)$"{LeaseKeyPrefix}:{key}";
        var leaseId = Guid.NewGuid();
        var leaseStr = leaseId.ToString("N");
        var success = await _db.StringSetAsync(redisKey, leaseStr, leaseTime, When.NotExists);
        if (!success) throw new InvalidOperationException($"Файл '{key}' уже обрабатывается другим процессом.");
        return leaseId;
    }

    public async Task ReleaseLeaseAsync(string fileName, Guid leaseId)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Имя файла не может быть пустым.", nameof(fileName));

        var redisKey = (RedisKey) $"{LeaseKeyPrefix}:{fileName}";
        var ok  = (long)await _releaseLoaded.EvaluateAsync(
            _db,
            new
            {
                leaseKey = redisKey,
                leaseId  = leaseId.ToString("N")
            });
        if (ok != 1)
        {
            // TODO: пересмотреть, когда будет добавлена поддержка пользователей (Слишком жесткое поведение при прокисании лицензии)
            throw new InvalidOperationException($"Не удалось снять блокировку файла '{fileName}'.");
        }
    }
}