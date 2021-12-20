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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal record MongoDbTransactionObjects(IMongoSession? MongoSession, IMongoClient MongoClient);

    internal class MongoDbTransactionRepositoryFactory<TEntity> : IMongoDbTransactionRepositoryFactory<TEntity>
    {
        private readonly IOptionsFactory<TransactionSessionOptions> _optionsFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IResolvingStrategyChain _resolvingStrategyChain;
        private readonly string _connectionString;

        protected readonly string _databaseName;

        public string DatabaseName => _databaseName;

        public MongoDbTransactionRepositoryFactory(IOptionsFactory<TransactionSessionOptions> optionsFactory, ILoggerFactory loggerFactory,
            IResolvingStrategyChain resolvingStrategyChain, string connectionString, string databaseName)
        {
            _optionsFactory = optionsFactory;
            _loggerFactory = loggerFactory;
            _resolvingStrategyChain = resolvingStrategyChain;
            _connectionString = connectionString;

            _databaseName = databaseName;
        }

        public IMongoClient CreatePrimaryClient()
        {
            return new MongoClient()
                .WithReadPreference(ReadPreference.Primary)
                .WithReadConcern(ReadConcern.Majority);
        }

        public IMongoClient CreateSecondaryClient()
        {
            return new MongoClient(_connectionString)
                .WithReadPreference(ReadPreference.SecondaryPreferred)
                .WithReadConcern(ReadConcern.Available);
        }

        public async Task<IClientSessionHandle> CreateClientSessionHandle()
        {
            var mongoClient = CreatePrimaryClient();

            var clientSessionHandle = await mongoClient.StartSessionAsync(new ClientSessionOptions
            {
                CausalConsistency = true,
                DefaultTransactionOptions = new TransactionOptions
                (
                    writeConcern: WriteConcern.WMajority
                )
            });

            return clientSessionHandle;
        }

        public TransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName)
        {
            return _optionsFactory.Create(transactionSessionOptionsName);
        }

        public ITransactionRepository<TEntity> CreateRepository
        (
            TransactionSessionOptions transactionSessionOptions,
            IMongoSession? mongoSession,
            IMongoClient mongoClient
        )
        {
            var logger = transactionSessionOptions.LoggerOverride ?? _loggerFactory.CreateLogger<TEntity>();

            var mongoDbTransactionRepository = new MongoDbTransactionRepository<TEntity>
            (
                mongoSession,
                mongoClient.GetDatabase(_databaseName),
                logger,
                _resolvingStrategyChain
            );

            return mongoDbTransactionRepository.UseTryCatch(logger);
        }

        public async Task<MongoDbTransactionObjects> CreateObjects(TransactionSessionOptions transactionSessionOptions)
        {
            if (transactionSessionOptions.ReadOnly)
            {
                if (transactionSessionOptions.SecondaryPreferred)
                {
                    return new(null, CreateSecondaryClient());
                }

                return new(null, CreatePrimaryClient());
            }

            var clientSessionHandle = await CreateClientSessionHandle();

            var mongoClient = clientSessionHandle.Client;

            var mongoSession = new MongoSession(clientSessionHandle);

            return new(mongoSession, mongoClient);
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
