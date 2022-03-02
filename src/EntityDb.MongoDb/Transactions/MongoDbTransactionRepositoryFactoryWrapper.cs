using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Disposables;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Sessions;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions;

internal abstract class MongoDbTransactionRepositoryFactoryWrapper<TEntity> : DisposableResourceBaseClass, IMongoDbTransactionRepositoryFactory<TEntity>
{
    private readonly IMongoDbTransactionRepositoryFactory<TEntity> _mongoDbTransactionRepositoryFactory;

    protected MongoDbTransactionRepositoryFactoryWrapper(
        IMongoDbTransactionRepositoryFactory<TEntity> mongoDbTransactionRepositoryFactory)
    {
        _mongoDbTransactionRepositoryFactory = mongoDbTransactionRepositoryFactory;
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

    public override async ValueTask DisposeAsync()
    {
        await _mongoDbTransactionRepositoryFactory.DisposeAsync();
    }
}
