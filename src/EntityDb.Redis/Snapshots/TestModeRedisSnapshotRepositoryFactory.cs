using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Strategies;
using EntityDb.Redis.Sessions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EntityDb.Redis.Snapshots
{
    internal sealed class TestModeRedisSnapshotRepositoryFactory<TEntity> : RedisSnapshotRepositoryFactory<TEntity>
    {
        public TestModeRedisSnapshotRepositoryFactory(ILoggerFactory loggerFactory,
            IResolvingStrategyChain resolvingStrategyChain, string connectionString, string keyNamespace) : base(
            loggerFactory, resolvingStrategyChain, connectionString, keyNamespace)
        {
        }

        internal override ISnapshotRepository<TEntity> CreateRepository(IRedisSession redisSession)
        {
            return new TestModeRedisSnapshotRepository<TEntity>(redisSession, _keyNamespace);
        }

        public static new TestModeRedisSnapshotRepositoryFactory<TEntity> Create(IServiceProvider serviceProvider,
            string connectionString, string keyNamespace)
        {
            return ActivatorUtilities.CreateInstance<TestModeRedisSnapshotRepositoryFactory<TEntity>>
            (
                serviceProvider,
                connectionString,
                keyNamespace
            );
        }
    }
}
