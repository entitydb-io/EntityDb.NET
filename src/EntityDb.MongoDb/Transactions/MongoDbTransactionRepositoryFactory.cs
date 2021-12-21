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
    internal class MongoDbTransactionRepositoryFactory<TEntity> : IMongoDbTransactionRepositoryFactory<TEntity>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptionsFactory<TransactionSessionOptions> _optionsFactory;
        private readonly IResolvingStrategyChain _resolvingStrategyChain;
        private readonly string _connectionString;
        private readonly string _databaseName;

        public MongoDbTransactionRepositoryFactory
        (
            IOptionsFactory<TransactionSessionOptions> optionsFactory,
            ILoggerFactory loggerFactory,
            IResolvingStrategyChain resolvingStrategyChain,
            string connectionString,
            string databaseName
        )
        {
            _optionsFactory = optionsFactory;
            _loggerFactory = loggerFactory;
            _resolvingStrategyChain = resolvingStrategyChain;
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        public ReadOnlyMongoSession CreateReadOnlySession(TransactionSessionOptions transactionSessionOptions)
        {
            var mongoDatabase = new MongoClient(_connectionString)
                .GetDatabase(_databaseName);

            return new ReadOnlyMongoSession(mongoDatabase, _loggerFactory, _resolvingStrategyChain, transactionSessionOptions);
        }

        public async Task<WriteMongoSession> CreateWriteSession(TransactionSessionOptions transactionSessionOptions)
        {
            var mongoClient = new MongoClient(_connectionString);
            var mongoDatabase = mongoClient.GetDatabase(_databaseName);

            var clientSessionHandle = await mongoClient.StartSessionAsync(new ClientSessionOptions
            {
                CausalConsistency = true,
                DefaultTransactionOptions = new TransactionOptions
                (
                    writeConcern: WriteConcern.WMajority
                )
            });

            return new WriteMongoSession(mongoDatabase, clientSessionHandle, _loggerFactory, _resolvingStrategyChain, transactionSessionOptions);
        }

        public TransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName)
        {
            return _optionsFactory.Create(transactionSessionOptionsName);
        }

        public async Task<IMongoSession> CreateSession(TransactionSessionOptions transactionSessionOptions)
        {
            if (transactionSessionOptions.ReadOnly)
            {
                return CreateReadOnlySession(transactionSessionOptions);
            }

            return await CreateWriteSession(transactionSessionOptions);
        }

        public ITransactionRepository<TEntity> CreateRepository
        (
            IMongoSession mongoSession
        )
        {
            var mongoDbTransactionRepository = new MongoDbTransactionRepository<TEntity>
            (
                mongoSession
            );

            return mongoDbTransactionRepository.UseTryCatch(mongoSession.Logger);
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
