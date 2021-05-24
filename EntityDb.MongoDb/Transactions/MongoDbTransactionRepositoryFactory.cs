using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Sessions;
using MongoDB.Driver;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal class MongoDbTransactionRepositoryFactory<TEntity> : ITransactionRepositoryFactory<TEntity>
    {
        private readonly string _connectionString;
        private readonly string _databaseName;

        protected readonly IServiceProvider _serviceProvider;

        public MongoDbTransactionRepositoryFactory(IServiceProvider serviceProvider, string connectionString, string databaseName)
        {
            _serviceProvider = serviceProvider;
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
        internal virtual IMongoDbSession CreateSession(IClientSessionHandle? clientSessionHandle, IMongoDatabase mongoDatabase)
        {
            return new MongoDbSession(_serviceProvider, clientSessionHandle, mongoDatabase);
        }

        public async Task<IMongoDbSession> CreateSession(ITransactionSessionOptions transactionSessionOptions)
        {
            var mongoClient = CreateClient(transactionSessionOptions);

            var mongoDatabase = mongoClient.GetDatabase(_databaseName);

            if (transactionSessionOptions.ReadOnly)
            {
                return CreateSession(null, mongoDatabase);
            }

            var clientSessionHandle = await CreateClientSessionHandle(mongoClient, transactionSessionOptions);

            return CreateSession(clientSessionHandle, mongoDatabase);
        }

        public async Task<ITransactionRepository<TEntity>> CreateRepository(ITransactionSessionOptions transactionSessionOptions)
        {
            var mongoDbSession = await CreateSession(transactionSessionOptions);

            return new MongoDbTransactionRepository<TEntity>(mongoDbSession);
        }
    }
}
