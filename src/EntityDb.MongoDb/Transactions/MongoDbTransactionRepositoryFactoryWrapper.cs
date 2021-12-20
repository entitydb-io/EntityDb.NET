using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Sessions;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal class MongoDbTransactionRepositoryFactoryWrapper<TEntity> : IMongoDbTransactionRepositoryFactory<TEntity>
    {
        private readonly IMongoDbTransactionRepositoryFactory<TEntity> _mongoDbTransactionRepositoryFactory;

        public MongoDbTransactionRepositoryFactoryWrapper(IMongoDbTransactionRepositoryFactory<TEntity> mongoDbTransactionRepositoryFactory)
        {
            _mongoDbTransactionRepositoryFactory = mongoDbTransactionRepositoryFactory;
        }

        public string DatabaseName => _mongoDbTransactionRepositoryFactory.DatabaseName;

        public virtual IMongoClient CreatePrimaryClient()
        {
            return _mongoDbTransactionRepositoryFactory.CreatePrimaryClient();
        }

        public virtual IMongoClient CreateSecondaryClient()
        {
            return _mongoDbTransactionRepositoryFactory.CreateSecondaryClient();
        }

        public virtual Task<IClientSessionHandle> CreateClientSessionHandle()
        {
            return _mongoDbTransactionRepositoryFactory.CreateClientSessionHandle();
        }

        public virtual TransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName)
        {
            return _mongoDbTransactionRepositoryFactory.GetTransactionSessionOptions(transactionSessionOptionsName);
        }

        public virtual Task<MongoDbTransactionObjects> CreateObjects(TransactionSessionOptions transactionSessionOptions)
        {
            return _mongoDbTransactionRepositoryFactory.CreateObjects(transactionSessionOptions);
        }

        public virtual ITransactionRepository<TEntity> CreateRepository
        (
            TransactionSessionOptions transactionSessionOptions,
            IMongoSession? mongoSession,
            IMongoClient mongoClient
        )
        {
            return _mongoDbTransactionRepositoryFactory.CreateRepository(transactionSessionOptions, mongoSession, mongoClient);
        }

        [ExcludeFromCodeCoverage(Justification = "Proxy for DisposeAsync")]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public virtual ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
