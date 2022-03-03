using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.TypeResolvers;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Redis.Envelopes;
using EntityDb.Redis.Sessions;
using StackExchange.Redis;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots;

internal class RedisSnapshotRepository<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
{
    private readonly IRedisSession _redisSession;
    private readonly string _keyNamespace;
    private readonly ILogger _logger;
    private readonly ITypeResolver _typeResolver;

    public RedisSnapshotRepository
    (
        string keyNamespace,
        ITypeResolver typeResolver,
        IRedisSession redisSession,
        ILogger logger
    )
    {
        _keyNamespace = keyNamespace;
        _typeResolver = typeResolver;
        _redisSession = redisSession;
        _logger = logger;
    }

    private RedisKey GetSnapshotKey(Id snapshotId)
    {
        return $"{_keyNamespace}#{snapshotId}";
    }

    public async Task<bool> PutSnapshot(Id snapshotId, TSnapshot snapshot)
    {
        var snapshotKey = GetSnapshotKey(snapshotId);

        var snapshotValue = JsonElementEnvelope
            .Deconstruct(snapshot, _logger)
            .Serialize(_logger);

        return await _redisSession.Insert(snapshotKey, snapshotValue);
    }

    public async Task<TSnapshot?> GetSnapshot(Id snapshotId)
    {
        var snapshotKey = GetSnapshotKey(snapshotId);
        var snapshotValue = await _redisSession.Find(snapshotKey);

        if (!snapshotValue.HasValue)
        {
            return default;
        }

        return JsonElementEnvelope
            .Deserialize(snapshotValue, _logger)
            .Reconstruct<TSnapshot>(_logger, _typeResolver);
    }

    public async Task<bool> DeleteSnapshots(Id[] snapshotIds)
    {
        var snapshotKeys = snapshotIds.Select(GetSnapshotKey);

        return await _redisSession.Delete(snapshotKeys);
    }

    public override async ValueTask DisposeAsync()
    {
        await _redisSession.DisposeAsync();
    }
}
