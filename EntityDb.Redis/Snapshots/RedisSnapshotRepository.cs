using EntityDb.Abstractions.Snapshots;
using EntityDb.Redis.Envelopes;
using EntityDb.Redis.Sessions;
using System;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots
{
    internal class RedisSnapshotRepository<TEntity> : ISnapshotRepository<TEntity>
    {
        private readonly string _keyNamespace;

        public IRedisSession RedisSession { get; }

        public RedisSnapshotRepository(IRedisSession redisSession, string keyNamespace)
        {
            RedisSession = redisSession;
            _keyNamespace = keyNamespace;
        }

        protected string GetKey(Guid entityId)
        {
            return $"{_keyNamespace}#{entityId}";
        }

        public virtual Task<bool> PutSnapshot(Guid entityId, TEntity entity)
        {
            return RedisSession.ExecuteCommand
            (
                (serviceProvider, redisTransaction) =>
                {
                    var jsonElementEnvelope = JsonElementEnvelope.Deconstruct(entity, serviceProvider);

                    var snapshotValue = jsonElementEnvelope.Serialize(serviceProvider);

                    var key = GetKey(entityId);

                    return redisTransaction.StringSetAsync(key, snapshotValue);
                }
            );
        }

        public virtual Task<TEntity?> GetSnapshot(Guid entityId)
        {
            return RedisSession.ExecuteQuery
            (
                async (serviceProvider, redisDatabase) =>
                {
                    var key = GetKey(entityId);

                    var snapshotValue = await redisDatabase.StringGetAsync(key);

                    if (snapshotValue.HasValue)
                    {
                        var jsonElementEnvelope = JsonElementEnvelope.Deserialize(snapshotValue, serviceProvider);

                        return jsonElementEnvelope.Reconstruct<TEntity>(serviceProvider);
                    }

                    return default;
                },
                default
            );
        }

        public async virtual ValueTask DisposeAsync()
        {
            await RedisSession.DisposeAsync();
        }
    }
}
