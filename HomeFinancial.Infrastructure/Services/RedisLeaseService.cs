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

    private readonly LoadedLuaScript _extendLoaded;
    private readonly LoadedLuaScript _releaseLoaded;

    public RedisLeaseService(IConnectionMultiplexer redis)
    {
        ArgumentNullException.ThrowIfNull(redis, nameof(redis));
        _db = redis.GetDatabase();
        
        // Загружаем скрипты на сервер
        var server = redis.GetServer(redis.GetEndPoints().First());
        _extendLoaded = ExtendScript.Load(server);
        _releaseLoaded = ReleaseScript.Load(server);
    }

    public async Task<Guid> AcquireLeaseAsync(string fileName, TimeSpan leaseTime)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName, nameof(fileName));

        var key = (RedisKey)$"{LeaseKeyPrefix}:{fileName}";
        var leaseId = Guid.NewGuid();
        var leaseStr = leaseId.ToString("N");
        var success = await _db.StringSetAsync(key, leaseStr, leaseTime, When.NotExists);
        if (!success) throw new InvalidOperationException($"Файл '{fileName}' уже обрабатывается другим процессом.");
        return leaseId;
    }

    public async Task ValidateAndExtendLeaseAsync(string fileName, Guid leaseId, TimeSpan leaseTime)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName, nameof(fileName));

        var key = $"{LeaseKeyPrefix}:{fileName}";
        var ok = (long)await _extendLoaded.EvaluateAsync(
            _db,
            new
            {
                leaseKey = key,                    
                leaseId  = leaseId.ToString("N"),
                ttl      = (long) leaseTime.TotalMilliseconds
            });

        if (ok != 1)
            throw new InvalidOperationException($"Сессия импорта файла «{fileName}» утрачена или истекла.");
    }

    public async Task ReleaseLeaseAsync(string fileName, Guid leaseId)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Имя файла не может быть пустым.", nameof(fileName));

        var key = $"{LeaseKeyPrefix}:{fileName}";
        var ok  = (long)await _releaseLoaded.EvaluateAsync(
            _db,
            new
            {
                leaseKey = (RedisKey)key,
                leaseId  = leaseId.ToString("N")
            });
        if (ok != 1)
        {
            // TODO: пересмотреть, когда будет добавлена поддержка пользователей (Слишком жесткое поведение при прокисании лицензии)
            throw new InvalidOperationException($"Не удалось снять блокировку файла '{fileName}'.");
        }
    }
}