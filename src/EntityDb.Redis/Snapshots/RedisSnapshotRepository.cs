using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.TypeResolvers;
using EntityDb.Common.Disposables;
using EntityDb.Redis.Envelopes;
using EntityDb.Redis.Sessions;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots;

internal class RedisSnapshotRepository<TEntity> : DisposableResourceBaseClass, ISnapshotRepository<TEntity>
{
    private readonly IRedisSession _redisSession;
    private readonly string _keyNamespace;
    private readonly ILogger _logger;
    private readonly ITypeResolver _typeResolver;
    private readonly ISnapshotStrategy<TEntity>? _snapshotStrategy;

    public RedisSnapshotRepository
    (
        string keyNamespace,
        ITypeResolver typeResolver,
        ISnapshotStrategy<TEntity>? snapshotStrategy,
        IRedisSession redisSession,
        ILogger logger
    )
    {
        _keyNamespace = keyNamespace;
        _typeResolver = typeResolver;
        _snapshotStrategy = snapshotStrategy;
        _redisSession = redisSession;
        _logger = logger;
    }

    private RedisKey GetSnapshotKey(Guid entityId)
    {
        return $"{_keyNamespace}#{entityId}";
    }

    public async Task<bool> PutSnapshot(Guid entityId, TEntity entity)
    {
        if (_snapshotStrategy != null)
        {
            var previousSnapshot = await GetSnapshot(entityId);

            if (!_snapshotStrategy.ShouldPutSnapshot(previousSnapshot, entity))
            {
                return false;
            }
        }

        var snapshotKey = GetSnapshotKey(entityId);

        var snapshotValue = JsonElementEnvelope
            .Deconstruct(entity, _logger)
            .Serialize(_logger);

        return await _redisSession.Insert(snapshotKey, snapshotValue);
    }

    public async Task<TEntity?> GetSnapshot(Guid entityId)
    {
        var snapshotKey = GetSnapshotKey(entityId);
        var snapshotValue = await _redisSession.Find(snapshotKey);

        if (!snapshotValue.HasValue)
        {
            return default;
        }

        return JsonElementEnvelope
            .Deserialize(snapshotValue, _logger)
            .Reconstruct<TEntity>(_logger, _typeResolver);
    }

    public async Task<bool> DeleteSnapshots(Guid[] entityIds)
    {
        var snapshotKeys = entityIds.Select(GetSnapshotKey);

        return await _redisSession.Delete(snapshotKeys);
    }

    public override async ValueTask DisposeAsync()
    {
        await _redisSession.DisposeAsync();
    }
}
