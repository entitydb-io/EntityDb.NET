using EntityDb.Abstractions.Snapshots;
using EntityDb.Redis.Envelopes;
using EntityDb.Redis.Sessions;
using StackExchange.Redis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots
{
    internal class RedisSnapshotRepository<TEntity> : ISnapshotRepository<TEntity>
    {
        private readonly string _keyNamespace;

        public RedisSnapshotRepository(IRedisSession redisSession, string keyNamespace)
        {
            RedisSession = redisSession;
            _keyNamespace = keyNamespace;
        }

        public IRedisSession RedisSession { get; }

        public virtual Task<bool> PutSnapshot(Guid entityId, TEntity entity)
        {
            return RedisSession.ExecuteCommand
            (
                (serviceProvider, redisTransaction) =>
                {
                    JsonElementEnvelope? jsonElementEnvelope = JsonElementEnvelope.Deconstruct(entity, serviceProvider);

                    byte[]? snapshotValue = jsonElementEnvelope.Serialize(serviceProvider);

                    string? key = GetKey(entityId);

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
                    string? key = GetKey(entityId);

                    RedisValue snapshotValue = await redisDatabase.StringGetAsync(key);

                    if (snapshotValue.HasValue)
                    {
                        JsonElementEnvelope? jsonElementEnvelope =
                            JsonElementEnvelope.Deserialize(snapshotValue, logger);

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
