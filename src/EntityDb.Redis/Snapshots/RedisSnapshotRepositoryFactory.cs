using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Extensions;
using EntityDb.Redis.Sessions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots
{
    internal class RedisSnapshotRepositoryFactory<TEntity> : ISnapshotRepositoryFactory<TEntity>
    {
        private readonly ILogger _logger;
        private readonly IResolvingStrategyChain _resolvingStrategyChain;
        private readonly string _connectionString;
        
        protected readonly string _keyNamespace;
        protected readonly ISnapshottingStrategy<TEntity>? _snapshottingStrategy;

        public RedisSnapshotRepositoryFactory(ILoggerFactory loggerFactory,
            IResolvingStrategyChain resolvingStrategyChain, string connectionString, string keyNamespace, ISnapshottingStrategy<TEntity>? snapshottingStrategy = null)
        {
            _logger = loggerFactory.CreateLogger<TEntity>();
            _resolvingStrategyChain = resolvingStrategyChain;
            _connectionString = connectionString;
            _keyNamespace = keyNamespace;
            _snapshottingStrategy = snapshottingStrategy;
        }

        public async Task<ISnapshotRepository<TEntity>> CreateRepository(ISnapshotSessionOptions snapshotSessionOptions)
        {
            var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(_connectionString);

            var redisSession = new RedisSession(connectionMultiplexer, _logger, _resolvingStrategyChain);

            return CreateRepository(redisSession);
        }

        [ExcludeFromCodeCoverage(Justification = "Tests use TestMode.")]
        internal virtual ISnapshotRepository<TEntity> CreateRepository(IRedisSession redisSession)
        {
            return new RedisSnapshotRepository<TEntity>(redisSession, _keyNamespace, _snapshottingStrategy);
        }

        public static RedisSnapshotRepositoryFactory<TEntity> Create(IServiceProvider serviceProvider,
            string connectionString, string keyNamespace)
        {
            return ActivatorUtilities.CreateInstance<RedisSnapshotRepositoryFactory<TEntity>>
            (
                serviceProvider,
                connectionString,
                keyNamespace
            );
        }
    }
}
