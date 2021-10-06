using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Strategies;
using EntityDb.Redis.Envelopes;
using EntityDb.Redis.Sessions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots
{
    internal class RedisSnapshotRepository<TEntity> : ISnapshotRepository<TEntity>
    {
        private readonly string _keyNamespace;
        private readonly ISnapshottingStrategy<TEntity>? _snapshottingStrategy;

        public RedisSnapshotRepository
        (
            IRedisSession redisSession,
            string keyNamespace,
            ISnapshottingStrategy<TEntity>? snapshottingStrategy = null
        )
        {
            RedisSession = redisSession;
            _keyNamespace = keyNamespace;
            _snapshottingStrategy = snapshottingStrategy;
        }

        public IRedisSession RedisSession { get; }

        public virtual async Task<bool> PutSnapshot(Guid entityId, TEntity entity)
        {
            if (_snapshottingStrategy != null)
            {
                var previousSnapshot = await GetSnapshot(entityId);

                if (_snapshottingStrategy.ShouldPutSnapshot(previousSnapshot, entity) == false)
                {
                    return false;
                }
            }
            
            return await RedisSession.ExecuteCommand
            (
                (logger, redisTransaction) =>
                {
                    var jsonElementEnvelope = JsonElementEnvelope.Deconstruct(entity, logger);

                    var snapshotValue = jsonElementEnvelope.Serialize(logger);

                    var key = GetKey(entityId);

                    return redisTransaction.StringSetAsync(key, snapshotValue);
                }
            );
        }

        public virtual Task<TEntity?> GetSnapshot(Guid entityId)
        {
            return RedisSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, redisDatabase) =>
                {
                    var key = GetKey(entityId);

                    var snapshotValue = await redisDatabase.StringGetAsync(key);

                    if (snapshotValue.HasValue)
                    {
                        var jsonElementEnvelope = JsonElementEnvelope.Deserialize(snapshotValue, logger);

                        return jsonElementEnvelope.Reconstruct<TEntity>(logger, resolvingStrategyChain);
                    }

                    return default;
                },
                default
            );
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public virtual async ValueTask DisposeAsync()
        {
            await RedisSession.DisposeAsync();
        }

        protected string GetKey(Guid entityId)
        {
            return $"{_keyNamespace}#{entityId}";
        }
    }
}
