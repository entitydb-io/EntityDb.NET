using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.MongoDb.Snapshots.Sessions;

namespace EntityDb.MongoDb.Snapshots;

internal abstract class MongoDbSnapshotRepositoryFactoryWrapper<TSnapshot> : DisposableResourceBaseClass,
    IMongoDbSnapshotRepositoryFactory<TSnapshot>
{
    private readonly IMongoDbSnapshotRepositoryFactory<TSnapshot> _mongoDbSnapshotRepositoryFactory;

    protected MongoDbSnapshotRepositoryFactoryWrapper(
        IMongoDbSnapshotRepositoryFactory<TSnapshot> mongoDbSnapshotRepositoryFactory)
    {
        _mongoDbSnapshotRepositoryFactory = mongoDbSnapshotRepositoryFactory;
    }

    public virtual MongoDbSnapshotSessionOptions GetSessionOptions(string snapshotSessionOptionsName)
    {
        return _mongoDbSnapshotRepositoryFactory.GetSessionOptions(snapshotSessionOptionsName);
    }

    public virtual Task<IMongoSession> CreateSession(MongoDbSnapshotSessionOptions options,
        CancellationToken cancellationToken)
    {
        return _mongoDbSnapshotRepositoryFactory.CreateSession(options, cancellationToken);
    }

    public virtual ISnapshotRepository<TSnapshot> CreateRepository
    (
        IMongoSession mongoSession
    )
    {
        return _mongoDbSnapshotRepositoryFactory.CreateRepository(mongoSession);
    }

    public override async ValueTask DisposeAsync()
    {
        await _mongoDbSnapshotRepositoryFactory.DisposeAsync();
    }
}
