using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.MongoDb.Documents;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Snapshots.Sessions;

internal record TestModeMongoSession(IMongoSession MongoSession) : DisposableResourceBaseRecord, IMongoSession
{
    public string CollectionName => MongoSession.CollectionName;

    public IMongoDatabase MongoDatabase => MongoSession.MongoDatabase;

    public Task Upsert(SnapshotDocument snapshotDocument, CancellationToken cancellationToken)
    {
        return MongoSession.Upsert(snapshotDocument, cancellationToken);
    }

    public Task<SnapshotDocument?> Find(Pointer snapshotPointer, CancellationToken cancellationToken)
    {
        return MongoSession.Find
        (
            snapshotPointer,
            cancellationToken
        );
    }

    public Task Delete(Pointer[] snapshotPointer, CancellationToken cancellationToken)
    {
        return MongoSession.Delete(snapshotPointer, cancellationToken);
    }

    public IMongoSession WithSessionOptions(MongoDbSnapshotSessionOptions options)
    {
        return new TestModeMongoSession(MongoSession.WithSessionOptions(options));
    }

    public void StartTransaction()
    {
        // Test Mode Snapshots are started in the Test Mode Repository Factory
    }

    public Task CommitTransaction(CancellationToken cancellationToken)
    {
        // Test Mode Snapshots are never committed
        return Task.CompletedTask;
    }

    public Task AbortTransaction()
    {
        // Test Mode Snapshots are aborted in the Test Mode Repository Factory
        return Task.CompletedTask;
    }
}
