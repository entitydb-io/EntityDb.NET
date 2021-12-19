using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal class MongoDbTransactionRepositoryFactory<TEntity> : ITransactionRepositoryFactory<TEntity>
    {
        private readonly string _connectionString;
        protected readonly string _databaseName;

        protected readonly IOptionsFactory<TransactionSessionOptions> _optionsFactory;
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly IResolvingStrategyChain _resolvingStrategyChain;

        public MongoDbTransactionRepositoryFactory(IOptionsFactory<TransactionSessionOptions> optionsFactory, ILoggerFactory loggerFactory,
            IResolvingStrategyChain resolvingStrategyChain, string connectionString, string databaseName)
        {
            _optionsFactory = optionsFactory;
            _loggerFactory = loggerFactory;
            _resolvingStrategyChain = resolvingStrategyChain;
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        public async Task<ITransactionRepository<TEntity>> CreateRepository(
            string transactionSessionOptionsName)
        {
            var transactionSessionOptions = _optionsFactory.Create(transactionSessionOptionsName);

            var mongoDbSession = await CreateSession(transactionSessionOptions);

            return new MongoDbTransactionRepository<TEntity>(mongoDbSession);
        }

        private static async Task<IClientSessionHandle> CreateClientSessionHandle(IMongoClient mongoClient,
            TransactionSessionOptions transactionSessionOptions)
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

        protected virtual Task<IMongoClient> CreateClient(TransactionSessionOptions transactionSessionOptions)
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

        protected virtual IMongoDbSession CreateSession(IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase, TransactionSessionOptions transactionSessionOptions)
        {
            if (transactionSessionOptions.TestMode)
            {
                return new TestModeMongoDbSession
                (
                    clientSessionHandle,
                    mongoDatabase,
                    transactionSessionOptions.LoggerOverride ?? _loggerFactory.CreateLogger<TEntity>(),
                    _resolvingStrategyChain
                );
            }

            return new MongoDbSession
            (
                clientSessionHandle,
                mongoDatabase,
                transactionSessionOptions.LoggerOverride ?? _loggerFactory.CreateLogger<TEntity>(),
                _resolvingStrategyChain
            );
        }

        private async Task<IMongoDbSession> CreateSession(TransactionSessionOptions transactionSessionOptions)
        {
            var mongoClient = await CreateClient(transactionSessionOptions);

            var mongoDatabase = mongoClient.GetDatabase(_databaseName);

            if (transactionSessionOptions.ReadOnly)
            {
                return CreateSession(null, mongoDatabase, transactionSessionOptions);
            }

            var clientSessionHandle = await CreateClientSessionHandle(mongoClient, transactionSessionOptions);

            return CreateSession(clientSessionHandle, mongoDatabase, transactionSessionOptions);
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
