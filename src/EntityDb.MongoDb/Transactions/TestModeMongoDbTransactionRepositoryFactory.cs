using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.MongoDb.Sessions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;

namespace EntityDb.MongoDb.Transactions
{
    internal class TestModeMongoDbTransactionRepositoryFactory<TEntity> : MongoDbTransactionRepositoryFactory<TEntity>
    {
        public TestModeMongoDbTransactionRepositoryFactory(ILoggerFactory loggerFactory,
            IResolvingStrategyChain resolvingStrategyChain, string connectionString, string databaseName) : base(
            loggerFactory, resolvingStrategyChain, connectionString, databaseName)
        {
        }

        protected override IMongoDbSession CreateSession(IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase, ILogger? loggerOverride)
        {
            return new TestModeMongoDbSession(clientSessionHandle, mongoDatabase, loggerOverride ?? _logger,
                _resolvingStrategyChain);
        }

        public static new TestModeMongoDbTransactionRepositoryFactory<TEntity> Create(IServiceProvider serviceProvider,
            string connectionString, string databaseName)
        {
            return ActivatorUtilities.CreateInstance<TestModeMongoDbTransactionRepositoryFactory<TEntity>>
            (
                serviceProvider,
                connectionString,
                databaseName
            );
        }
    }
}
