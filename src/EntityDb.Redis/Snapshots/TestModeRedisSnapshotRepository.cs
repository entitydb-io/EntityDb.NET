using EntityDb.Abstractions.Strategies;
using EntityDb.Redis.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots
{
    internal sealed class TestModeRedisSnapshotRepository<TEntity> : RedisSnapshotRepository<TEntity>
    {
        private readonly TestModeRedisSnapshotRepositoryDisposer _disposer;

        public TestModeRedisSnapshotRepository
        (
            TestModeRedisSnapshotRepositoryDisposer disposer,
            IRedisSession redisSession,
            string keyNamespace,
            ISnapshottingStrategy<TEntity>? snapshottingStrategy = null
        ) : base(redisSession, keyNamespace, snapshottingStrategy)
        {
            _disposer = disposer;

            _disposer.Lock();
        }

        public override Task<bool> PutSnapshot(Guid entityId, TEntity entity)
        {
            _disposer.AddDisposeId(entityId);

            return base.PutSnapshot(entityId, entity);
        }

        public override async ValueTask DisposeAsync()
        {
            _disposer.Release();

            if (_disposer.TryDispose(out var disposeIds) == false)
            {
                return;
            }
            
            await RedisSession.ExecuteCommand
            (
                (_, redisTransaction) =>
                {
                    var tasks = disposeIds
                        .Select(GetKey)
                        .Select(key => redisTransaction.KeyDeleteAsync(key))
                        .ToArray();

                    return Task.Run(async () =>
                    {
                        await Task.WhenAll(tasks);

                        return tasks.All(task => task.Result);
                    });
                }
            );

            await base.DisposeAsync();
        }
    }
}
