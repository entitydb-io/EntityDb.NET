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
        protected ILogger _logger;
        protected IResolvingStrategyChain _resolvingStrategyChain;
        protected readonly string _connectionString;
        protected readonly string _keyNamespace;

        public RedisSnapshotRepositoryFactory(ILoggerFactory loggerFactory, IResolvingStrategyChain resolvingStrategyChain, string connectionString, string keyNamespace)
        {
            _logger = loggerFactory.CreateLogger<TEntity>();
            _resolvingStrategyChain = resolvingStrategyChain;
            _connectionString = connectionString;
            _keyNamespace = keyNamespace;
        }

        [ExcludeFromCodeCoverage(Justification = "Tests use TestMode.")]
        internal virtual ISnapshotRepository<TEntity> CreateRepository(IRedisSession redisSession)
        {
            return new RedisSnapshotRepository<TEntity>(redisSession, _keyNamespace);
        }

        public async Task<ISnapshotRepository<TEntity>> CreateRepository(ISnapshotSessionOptions snapshotSessionOptions)
        {
            var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(_connectionString);

            var redisSession = new RedisSession(connectionMultiplexer, _logger, _resolvingStrategyChain);

            return CreateRepository(redisSession);
        }

        public static RedisSnapshotRepositoryFactory<TEntity> Create(IServiceProvider serviceProvider, string connectionString, string keyNamespace)
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
