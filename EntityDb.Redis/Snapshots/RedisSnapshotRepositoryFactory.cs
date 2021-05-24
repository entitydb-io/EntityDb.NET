using EntityDb.Abstractions.Snapshots;
using EntityDb.Redis.Sessions;
using StackExchange.Redis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.Redis.Snapshots
{
    internal class RedisSnapshotRepositoryFactory<TEntity> : ISnapshotRepositoryFactory<TEntity>
    {
        private readonly string _connectionString;

        protected readonly IServiceProvider _serviceProvider;
        protected readonly string _keyNamespace;

        public RedisSnapshotRepositoryFactory(IServiceProvider serviceProvider, string connectionString, string keyNamespace)
        {
            _serviceProvider = serviceProvider;
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

            var redisSession = new RedisSession(_serviceProvider, connectionMultiplexer);

            return CreateRepository(redisSession);
        }
    }
}
