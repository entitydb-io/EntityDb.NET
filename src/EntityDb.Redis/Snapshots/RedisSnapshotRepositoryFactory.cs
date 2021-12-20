using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots
{
    internal class RedisSnapshotRepositoryFactory<TEntity> : ISnapshotRepositoryFactory<TEntity>
    {
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

            var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(_connectionString);

            var logger = snapshotSessionOptions.LoggerOverride ?? _loggerFactory.CreateLogger<TEntity>();

            var redisSnapshotRepository = new RedisSnapshotRepository<TEntity>
            (
                _keyNamespace,
                _resolvingStrategyChain,
                _snapshottingStrategy,
                connectionMultiplexer,
                logger
            );

            return redisSnapshotRepository.UseTryCatch(logger);
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

        [ExcludeFromCodeCoverage(Justification = "Proxy for DisposeAsync")]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
