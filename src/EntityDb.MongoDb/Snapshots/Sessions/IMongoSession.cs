using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Snapshots.Sessions;

internal interface IMongoSession : IDisposableResource
{
    IMongoDatabase MongoDatabase { get; }
    string CollectionName { get; }

    Task Upsert(SnapshotDocument snapshotDocument, CancellationToken cancellationToken);

    Task<SnapshotDocument?> Find(Pointer snapshotPointer, CancellationToken cancellationToken);

    Task Delete(Pointer[] snapshotPointers, CancellationToken cancellationToken);

    void StartTransaction();
    Task CommitTransaction(CancellationToken cancellationToken);
    Task AbortTransaction();

    IMongoSession WithSessionOptions(MongoDbSnapshotSessionOptions options);
}
