using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Driver;

namespace EntityDb.MongoDb.States.Sessions;

internal interface IMongoSession : IDisposableResource
{
    IMongoDatabase MongoDatabase { get; }
    string CollectionName { get; }

    Task Upsert(StateDocument stateDocument, CancellationToken cancellationToken);

    Task<StateDocument?> Fetch(Pointer statePointer, CancellationToken cancellationToken);

    Task Delete(Pointer[] statePointer, CancellationToken cancellationToken);

    void StartTransaction();
    Task CommitTransaction(CancellationToken cancellationToken);
    Task AbortTransaction();

    IMongoSession WithSessionOptions(MongoDbStateSessionOptions options);
}
