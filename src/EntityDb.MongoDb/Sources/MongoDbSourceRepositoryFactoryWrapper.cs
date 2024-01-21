using EntityDb.Abstractions.Sources;
using EntityDb.Common.Disposables;
using EntityDb.MongoDb.Sources.Sessions;

namespace EntityDb.MongoDb.Sources;

internal abstract class MongoDbSourceRepositoryFactoryWrapper : DisposableResourceBaseClass,
    IMongoDbSourceRepositoryFactory
{
    private readonly IMongoDbSourceRepositoryFactory _mongoDbSourceRepositoryFactory;

    protected MongoDbSourceRepositoryFactoryWrapper(
        IMongoDbSourceRepositoryFactory mongoDbSourceRepositoryFactory)
    {
        _mongoDbSourceRepositoryFactory = mongoDbSourceRepositoryFactory;
    }

    public virtual MongoDbSourceSessionOptions GetSourceSessionOptions(string sourceSessionOptionsName)
    {
        return _mongoDbSourceRepositoryFactory.GetSourceSessionOptions(sourceSessionOptionsName);
    }

    public virtual Task<IMongoSession> CreateSession(MongoDbSourceSessionOptions options,
        CancellationToken cancellationToken)
    {
        return _mongoDbSourceRepositoryFactory.CreateSession(options, cancellationToken);
    }

    public virtual ISourceRepository CreateRepository
    (
        IMongoSession mongoSession
    )
    {
        return _mongoDbSourceRepositoryFactory.CreateRepository(mongoSession);
    }

    public override async ValueTask DisposeAsync()
    {
        await _mongoDbSourceRepositoryFactory.DisposeAsync();
    }
}
