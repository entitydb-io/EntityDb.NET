using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using EntityDb.Redis.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots
{
    internal class RedisSnapshotRepositoryFactory<TEntity> : ISnapshotRepositoryFactory<TEntity>
    {
        private readonly TestModeRedisSnapshotRepositoryDisposer _disposer = new();

        private readonly IOptionsFactory<SnapshotSessionOptions> _optionsFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IResolvingStrategyChain _resolvingStrategyChain;
        private readonly string _connectionString;

        protected readonly string _keyNamespace;
        protected readonly ISnapshottingStrategy<TEntity>? _snapshottingStrategy;

        public RedisSnapshotRepositoryFactory(IOptionsFactory<SnapshotSessionOptions> optionsFactory, ILoggerFactory loggerFactory,
            IResolvingStrategyChain resolvingStrategyChain, string connectionString, string keyNamespace, ISnapshottingStrategy<TEntity>? snapshottingStrategy = null)
        {
            _optionsFactory = optionsFactory;
            _loggerFactory = loggerFactory;
            _resolvingStrategyChain = resolvingStrategyChain;
            _connectionString = connectionString;
            _keyNamespace = keyNamespace;
            _snapshottingStrategy = snapshottingStrategy;
        }

        public async Task<ISnapshotRepository<TEntity>> CreateRepository(string snapshotSessionOptionsName)
        {
            var snapshotSessionOptions = _optionsFactory.Create(snapshotSessionOptionsName);

            if (snapshotSessionOptions.TestMode == false)
            {
                throw new Exception("Do not expect tests to run in non-test mode.");
            }

            var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(_connectionString);

            var redisSession = new RedisSession
            (
                connectionMultiplexer,
                snapshotSessionOptions.LoggerOverride ?? _loggerFactory.CreateLogger<TEntity>(),
                _resolvingStrategyChain
            );

            return CreateRepository(redisSession, snapshotSessionOptions);
        }

        internal virtual ISnapshotRepository<TEntity> CreateRepository(IRedisSession redisSession, SnapshotSessionOptions snapshotSessionOptions)
        {
            if (snapshotSessionOptions.TestMode)
            {
                return new TestModeRedisSnapshotRepository<TEntity>(_disposer, redisSession, _keyNamespace, _snapshottingStrategy);
            }

            throw new Exception("Do not expect tests to run in non-test mode.");

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
