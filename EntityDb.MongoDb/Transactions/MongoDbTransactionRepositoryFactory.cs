using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Sessions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal class MongoDbTransactionRepositoryFactory<TEntity> : ITransactionRepositoryFactory<TEntity>
    {
        protected readonly ILogger _logger;
        protected readonly IResolvingStrategyChain _resolvingStrategyChain;
        protected readonly string _connectionString;
        protected readonly string _databaseName;

        public MongoDbTransactionRepositoryFactory(ILoggerFactory loggerFactory, IResolvingStrategyChain resolvingStrategyChain, string connectionString, string databaseName)
        {
            _logger = loggerFactory.CreateLogger<TEntity>();
            _resolvingStrategyChain = resolvingStrategyChain;
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        private static async Task<IClientSessionHandle> CreateClientSessionHandle(IMongoClient mongoClient, ITransactionSessionOptions transactionSessionOptions)
        {
            return await mongoClient.StartSessionAsync(new ClientSessionOptions
            {
                CausalConsistency = true,
                DefaultTransactionOptions = new
                (
                    writeConcern: WriteConcern.WMajority,
                    maxCommitTime: transactionSessionOptions.WriteTimeout
                ),
            });
        }

        private IMongoClient CreateClient(ITransactionSessionOptions transactionSessionOptions)
        {
            var mongoClient = new MongoClient(_connectionString);

            if (transactionSessionOptions.SecondaryPreferred)
            {
                return mongoClient
                    .WithReadPreference(ReadPreference.SecondaryPreferred)
                    .WithReadConcern(ReadConcern.Available);
            }

            return mongoClient
                .WithReadPreference(ReadPreference.Primary)
                .WithReadConcern(ReadConcern.Majority);
        }

        [ExcludeFromCodeCoverage(Justification = "Tests use TestMode.")]
        internal virtual IMongoDbSession CreateSession(IClientSessionHandle? clientSessionHandle, IMongoDatabase mongoDatabase, ILogger? loggerOverride)
        {
            return new MongoDbSession(clientSessionHandle, mongoDatabase, loggerOverride ?? _logger, _resolvingStrategyChain);
        }

        public async Task<IMongoDbSession> CreateSession(ITransactionSessionOptions transactionSessionOptions)
        {
            var mongoClient = CreateClient(transactionSessionOptions);

            var mongoDatabase = mongoClient.GetDatabase(_databaseName);

            if (transactionSessionOptions.ReadOnly)
            {
                return CreateSession(null, mongoDatabase, transactionSessionOptions.LoggerOverride);
            }

            var clientSessionHandle = await CreateClientSessionHandle(mongoClient, transactionSessionOptions);

            return CreateSession(clientSessionHandle, mongoDatabase, transactionSessionOptions.LoggerOverride);
        }

        public async Task<ITransactionRepository<TEntity>> CreateRepository(ITransactionSessionOptions transactionSessionOptions)
        {
            var mongoDbSession = await CreateSession(transactionSessionOptions);

            return new MongoDbTransactionRepository<TEntity>(mongoDbSession);
        }

        public static MongoDbTransactionRepositoryFactory<TEntity> Create(IServiceProvider serviceProvider, string connectionString, string databaseName)
        {
            return ActivatorUtilities.CreateInstance<MongoDbTransactionRepositoryFactory<TEntity>>
            (
                serviceProvider,
                connectionString,
                databaseName
            );
        }
    }
}
