using EntityDb.Abstractions.Snapshots;
using EntityDb.MongoDb.Snapshots.Sessions;

namespace EntityDb.MongoDb.Snapshots;

internal interface IMongoDbSnapshotRepositoryFactory<TSnapshot> : ISnapshotRepositoryFactory<TSnapshot>
{
    async Task<ISnapshotRepository<TSnapshot>> ISnapshotRepositoryFactory<TSnapshot>.CreateRepository(
        string snapshotSessionOptionsName, CancellationToken cancellationToken)
    {
        var options = GetSessionOptions(snapshotSessionOptionsName);

        var mongoSession = await CreateSession(options, cancellationToken);

        return CreateRepository(mongoSession);
    }

    MongoDbSnapshotSessionOptions GetSessionOptions(string snapshotSessionOptionsName);

    Task<IMongoSession> CreateSession(MongoDbSnapshotSessionOptions options,
        CancellationToken cancellationToken);

    ISnapshotRepository<TSnapshot> CreateRepository(IMongoSession mongoSession);
}
