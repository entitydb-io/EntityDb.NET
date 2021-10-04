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
        private readonly string _connectionString;
        protected readonly string _databaseName;

        protected readonly ILogger _logger;
        protected readonly IResolvingStrategyChain _resolvingStrategyChain;

        public MongoDbTransactionRepositoryFactory(ILoggerFactory loggerFactory,
            IResolvingStrategyChain resolvingStrategyChain, string connectionString, string databaseName)
        {
            _logger = loggerFactory.CreateLogger<TEntity>();
            _resolvingStrategyChain = resolvingStrategyChain;
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        public async Task<ITransactionRepository<TEntity>> CreateRepository(
            ITransactionSessionOptions transactionSessionOptions)
        {
            var mongoDbSession = await CreateSession(transactionSessionOptions);

            return new MongoDbTransactionRepository<TEntity>(mongoDbSession);
        }

        private static async Task<IClientSessionHandle> CreateClientSessionHandle(IMongoClient mongoClient,
            ITransactionSessionOptions transactionSessionOptions)
        {
            return await mongoClient.StartSessionAsync(new ClientSessionOptions
            {
                CausalConsistency = true,
                DefaultTransactionOptions = new TransactionOptions(
                    writeConcern: WriteConcern.WMajority,
                    maxCommitTime: transactionSessionOptions.WriteTimeout
                )
            });
        }

        protected virtual Task<IMongoClient> CreateClient(ITransactionSessionOptions transactionSessionOptions)
        {
            IMongoClient mongoClient = new MongoClient(_connectionString);

            if (transactionSessionOptions.SecondaryPreferred)
            {
                mongoClient = mongoClient
                    .WithReadPreference(ReadPreference.SecondaryPreferred)
                    .WithReadConcern(ReadConcern.Available);
            }
            else
            {
                mongoClient = mongoClient
                    .WithReadPreference(ReadPreference.Primary)
                    .WithReadConcern(ReadConcern.Majority);
            }

            return Task.FromResult(mongoClient);
        }

        [ExcludeFromCodeCoverage(Justification = "Tests use TestMode.")]
        protected virtual IMongoDbSession CreateSession(IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase, ILogger? loggerOverride)
        {
            return new MongoDbSession(clientSessionHandle, mongoDatabase, loggerOverride ?? _logger,
                _resolvingStrategyChain);
        }

        private async Task<IMongoDbSession> CreateSession(ITransactionSessionOptions transactionSessionOptions)
        {
            var mongoClient = await CreateClient(transactionSessionOptions);

            var mongoDatabase = mongoClient.GetDatabase(_databaseName);

            if (transactionSessionOptions.ReadOnly)
            {
                return CreateSession(null, mongoDatabase, transactionSessionOptions.LoggerOverride);
            }

            var clientSessionHandle = await CreateClientSessionHandle(mongoClient, transactionSessionOptions);

            return CreateSession(clientSessionHandle, mongoDatabase, transactionSessionOptions.LoggerOverride);
        }

        public static MongoDbTransactionRepositoryFactory<TEntity> Create(IServiceProvider serviceProvider,
            string connectionString, string databaseName)
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
