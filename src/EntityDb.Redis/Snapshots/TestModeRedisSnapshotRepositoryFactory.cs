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
        private readonly TestModeRedisSnapshotRepositoryDisposer _disposer = new();
        
        public TestModeRedisSnapshotRepositoryFactory(ILoggerFactory loggerFactory,
            IResolvingStrategyChain resolvingStrategyChain, string connectionString, string keyNamespace, ISnapshottingStrategy<TEntity>? snapshottingStrategy = null) : base(
            loggerFactory, resolvingStrategyChain, connectionString, keyNamespace, snapshottingStrategy)
        {
        }

        internal override ISnapshotRepository<TEntity> CreateRepository(IRedisSession redisSession)
        {
            return new TestModeRedisSnapshotRepository<TEntity>(_disposer, redisSession, _keyNamespace, _snapshottingStrategy);
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
