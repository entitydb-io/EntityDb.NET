using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Disposables;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Sessions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions;

internal abstract class MongoDbTransactionRepositoryFactoryWrapper : DisposableResourceBaseClass, IMongoDbTransactionRepositoryFactory
{
    private readonly IMongoDbTransactionRepositoryFactory _mongoDbTransactionRepositoryFactory;

    protected MongoDbTransactionRepositoryFactoryWrapper(
        IMongoDbTransactionRepositoryFactory mongoDbTransactionRepositoryFactory)
    {
        _mongoDbTransactionRepositoryFactory = mongoDbTransactionRepositoryFactory;
    }

    public virtual TransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName)
    {
        return _mongoDbTransactionRepositoryFactory.GetTransactionSessionOptions(transactionSessionOptionsName);
    }

    public virtual Task<IMongoSession> CreateSession(TransactionSessionOptions transactionSessionOptions, CancellationToken cancellationToken)
    {
        return _mongoDbTransactionRepositoryFactory.CreateSession(transactionSessionOptions, cancellationToken);
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
