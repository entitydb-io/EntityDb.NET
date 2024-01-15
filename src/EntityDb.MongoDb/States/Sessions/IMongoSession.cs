using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.States;
using EntityDb.MongoDb.Documents;
using MongoDB.Driver;

namespace EntityDb.MongoDb.States.Sessions;

internal interface IMongoSession : IDisposableResource
{
    IMongoDatabase MongoDatabase { get; }
    string CollectionName { get; }

    Task Upsert(StateDocument stateDocument, CancellationToken cancellationToken);

    Task<StateDocument?> Fetch(StatePointer statePointer, CancellationToken cancellationToken);

    Task Delete(StatePointer[] statePointer, CancellationToken cancellationToken);

    void StartTransaction();
    Task CommitTransaction(CancellationToken cancellationToken);
    Task AbortTransaction();

    IMongoSession WithSessionOptions(MongoDbStateSessionOptions options);
}
