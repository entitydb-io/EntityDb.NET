using EntityDb.Redis.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots
{
    internal sealed class TestModeRedisSnapshotRepository<TEntity> : RedisSnapshotRepository<TEntity>
    {
        private readonly List<Guid> _disposeEntityIds = new();

        public TestModeRedisSnapshotRepository(IRedisSession redisSession, string keyNamespace) : base(redisSession, keyNamespace)
        {
        }

        public override Task<bool> PutSnapshot(Guid entityId, TEntity entity)
        {
            _disposeEntityIds.Add(entityId);

            return base.PutSnapshot(entityId, entity);
        }

        public async override ValueTask DisposeAsync()
        {
            await RedisSession.ExecuteCommand
            (
                (serviceProvider, redisTransaction) =>
                {
                    var tasks = _disposeEntityIds
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
