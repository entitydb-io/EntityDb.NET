using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Sessions;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal abstract class MongoDbTransactionRepositoryFactoryWrapper<TEntity> : IMongoDbTransactionRepositoryFactory<TEntity>
    {
        protected readonly IMongoDbTransactionRepositoryFactory<TEntity> _mongoDbTransactionRepositoryFactory;

        protected MongoDbTransactionRepositoryFactoryWrapper(
            IMongoDbTransactionRepositoryFactory<TEntity> mongoDbTransactionRepositoryFactory)
        {
            _mongoDbTransactionRepositoryFactory = mongoDbTransactionRepositoryFactory;
        }

        public virtual ReadOnlyMongoSession CreateReadOnlySession(TransactionSessionOptions transactionSessionOptions)
        {
            return _mongoDbTransactionRepositoryFactory.CreateReadOnlySession(transactionSessionOptions);
        }

        public virtual Task<WriteMongoSession> CreateWriteSession(TransactionSessionOptions transactionSessionOptions)
        {
            return _mongoDbTransactionRepositoryFactory.CreateWriteSession(transactionSessionOptions);
        }

        public virtual TransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName)
        {
            return _mongoDbTransactionRepositoryFactory.GetTransactionSessionOptions(transactionSessionOptionsName);
        }

        public virtual Task<IMongoSession> CreateSession(TransactionSessionOptions transactionSessionOptions)
        {
            return _mongoDbTransactionRepositoryFactory.CreateSession(transactionSessionOptions);
        }

        public virtual ITransactionRepository<TEntity> CreateRepository
        (
            IMongoSession mongoSession
        )
        {
            return _mongoDbTransactionRepositoryFactory.CreateRepository(mongoSession);
        }

        [ExcludeFromCodeCoverage(Justification = "Proxy for DisposeAsync")]
        public virtual void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public virtual ValueTask DisposeAsync()
        {
            return _mongoDbTransactionRepositoryFactory.DisposeAsync();
        }
    }
}
