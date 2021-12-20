using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Strategies;
using EntityDb.Redis.Envelopes;
using StackExchange.Redis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Redis.Sessions
{
    internal class RedisSnapshotRepository<TEntity> : ISnapshotRepository<TEntity>
    {
        private readonly string _keyNamespace;
        protected readonly IResolvingStrategyChain _resolvingStrategyChain;
        private readonly ISnapshottingStrategy<TEntity>? _snapshottingStrategy;
        protected readonly IConnectionMultiplexer _connectionMultiplexer;
        protected readonly ILogger _logger;

        public RedisSnapshotRepository
        (
            string keyNamespace,
            IResolvingStrategyChain resolvingStrategyChain,
            ISnapshottingStrategy<TEntity>? snapshottingStrategy,
            IConnectionMultiplexer connectionMultiplexer,
            ILogger logger
        )
        {
            _keyNamespace = keyNamespace;
            _resolvingStrategyChain = resolvingStrategyChain;
            _snapshottingStrategy = snapshottingStrategy;
            _connectionMultiplexer = connectionMultiplexer;
            _logger = logger;
        }

        public RedisKey GetSnapshotKey(Guid entityId)
        {
            return $"{_keyNamespace}#{entityId}";
        }

        public async Task<bool> PutSnapshot(Guid entityId, TEntity entity)
        {
            if (_snapshottingStrategy != null)
            {
                var previousSnapshot = await GetSnapshot(entityId);

                if (_snapshottingStrategy.ShouldPutSnapshot(previousSnapshot, entity) == false)
                {
                    return false;
                }
            }

            var snapshotKey = GetSnapshotKey(entityId);

            var snapshotValue = JsonElementEnvelope
                .Deconstruct(entity, _logger)
                .Serialize(_logger);

            var redisTransaction = _connectionMultiplexer.GetDatabase().CreateTransaction();

            var insertedTask = redisTransaction.StringSetAsync(snapshotKey, snapshotValue);

            await redisTransaction.ExecuteAsync();

            return await insertedTask;
        }

        public async Task<TEntity?> GetSnapshot(Guid entityId)
        {
            var snapshotKey = GetSnapshotKey(entityId);

            var redisDatabase = _connectionMultiplexer.GetDatabase();

            var snapshotValue = await redisDatabase.StringGetAsync(snapshotKey);

            if (snapshotValue.HasValue == false)
            {
                return default;
            }

            return JsonElementEnvelope
                .Deserialize(snapshotValue, _logger)
                .Reconstruct<TEntity>(_logger, _resolvingStrategyChain);
        }

        public async Task<bool> DeleteSnapshots(Guid[] entityIds)
        {
            var redisTransaction = _connectionMultiplexer.GetDatabase().CreateTransaction();

            var deleteSnapshotTasks = entityIds
                .Select(GetSnapshotKey)
                .Select(key => redisTransaction.KeyDeleteAsync(key))
                .ToArray();

            await redisTransaction.ExecuteAsync();

            await Task.WhenAll(deleteSnapshotTasks);

            return deleteSnapshotTasks
                .All(task => task.Result);
        }

        [ExcludeFromCodeCoverage(Justification = "Proxy for DisposeAsync")]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            await Task.Yield();

            _connectionMultiplexer.Dispose();
        }
    }
}
