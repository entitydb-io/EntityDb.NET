using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.MongoDb.Documents;
using MongoDB.Driver;

namespace EntityDb.MongoDb.States.Sessions;

internal record TestModeMongoSession(IMongoSession MongoSession) : DisposableResourceBaseRecord, IMongoSession
{
    public string CollectionName => MongoSession.CollectionName;

    public IMongoDatabase MongoDatabase => MongoSession.MongoDatabase;

    public Task Upsert(StateDocument stateDocument, CancellationToken cancellationToken)
    {
        return MongoSession.Upsert(stateDocument, cancellationToken);
    }

    public Task<StateDocument?> Fetch(Pointer statePointer, CancellationToken cancellationToken)
    {
        return MongoSession.Fetch
        (
            statePointer,
            cancellationToken
        );
    }

    public Task Delete(Pointer[] statePointer, CancellationToken cancellationToken)
    {
        return MongoSession.Delete(statePointer, cancellationToken);
    }

    public IMongoSession WithSessionOptions(MongoDbStateSessionOptions options)
    {
        return new TestModeMongoSession(MongoSession.WithSessionOptions(options));
    }

    public void StartTransaction()
    {
        // Test Mode Transactions are started in the Test Mode Repository Factory
    }

    public Task CommitTransaction(CancellationToken cancellationToken)
    {
        // Test Mode Transactions are never committed
        return Task.CompletedTask;
    }

    public Task AbortTransaction()
    {
        // Test Mode Transactions are aborted in the Test Mode Repository Factory
        return Task.CompletedTask;
    }
}
