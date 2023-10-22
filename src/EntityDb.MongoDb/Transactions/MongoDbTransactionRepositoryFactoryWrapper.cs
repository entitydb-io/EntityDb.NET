using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Disposables;
using EntityDb.MongoDb.Transactions.Sessions;

namespace EntityDb.MongoDb.Transactions;

internal abstract class MongoDbTransactionRepositoryFactoryWrapper : DisposableResourceBaseClass,
    IMongoDbTransactionRepositoryFactory
{
    private readonly IMongoDbTransactionRepositoryFactory _mongoDbTransactionRepositoryFactory;

    protected MongoDbTransactionRepositoryFactoryWrapper(
        IMongoDbTransactionRepositoryFactory mongoDbTransactionRepositoryFactory)
    {
        _mongoDbTransactionRepositoryFactory = mongoDbTransactionRepositoryFactory;
    }

    public virtual MongoDbTransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName)
    {
        return _mongoDbTransactionRepositoryFactory.GetTransactionSessionOptions(transactionSessionOptionsName);
    }

    public virtual Task<IMongoSession> CreateSession(MongoDbTransactionSessionOptions options,
        CancellationToken cancellationToken)
    {
        return _mongoDbTransactionRepositoryFactory.CreateSession(options, cancellationToken);
    }

    public virtual ITransactionRepository CreateRepository
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
