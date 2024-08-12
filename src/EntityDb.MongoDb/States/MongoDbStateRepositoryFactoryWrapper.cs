using EntityDb.Abstractions.States;
using EntityDb.Common.Disposables;
using EntityDb.MongoDb.States.Sessions;

namespace EntityDb.MongoDb.States;

internal abstract class MongoDbStateRepositoryFactoryWrapper<TState> : DisposableResourceBaseClass,
    IMongoDbStateRepositoryFactory<TState>
{
    private readonly IMongoDbStateRepositoryFactory<TState> _mongoDbStateRepositoryFactory;

    protected MongoDbStateRepositoryFactoryWrapper(
        IMongoDbStateRepositoryFactory<TState> mongoDbStateRepositoryFactory)
    {
        _mongoDbStateRepositoryFactory = mongoDbStateRepositoryFactory;
    }

    public virtual MongoDbStateSessionOptions GetSessionOptions(string stateSessionOptionsName)
    {
        return _mongoDbStateRepositoryFactory.GetSessionOptions(stateSessionOptionsName);
    }

    public virtual Task<IMongoSession> CreateSession(MongoDbStateSessionOptions options,
        CancellationToken cancellationToken)
    {
        return _mongoDbStateRepositoryFactory.CreateSession(options, cancellationToken);
    }

    public virtual IStateRepository<TState> CreateRepository
    (
        IMongoSession mongoSession
    )
    {
        return _mongoDbStateRepositoryFactory.CreateRepository(mongoSession);
    }

    public override async ValueTask DisposeAsync()
    {
        await _mongoDbStateRepositoryFactory.DisposeAsync();
    }
}
