using EntityDb.Abstractions.Snapshots;
using EntityDb.Redis.Sessions;
using System;

namespace EntityDb.Redis.Snapshots
{
    internal sealed class TestModeRedisSnapshotRepositoryFactory<TEntity> : RedisSnapshotRepositoryFactory<TEntity>
    {
        public TestModeRedisSnapshotRepositoryFactory(IServiceProvider serviceProvider, string connectionString, string keyNamespace) : base(serviceProvider, connectionString, keyNamespace)
        {
        }

        internal override ISnapshotRepository<TEntity> CreateRepository(IRedisSession redisSession)
        {
            return new TestModeRedisSnapshotRepository<TEntity>(redisSession, _keyNamespace);
        }
    }
}
