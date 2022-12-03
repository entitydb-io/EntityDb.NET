using EntityDb.Common.Disposables;
using EntityDb.MongoDb.Queries;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sessions;

internal record TestModeMongoSession(IMongoSession MongoSession) : DisposableResourceBaseRecord, IMongoSession
{
    public IMongoDatabase MongoDatabase => MongoSession.MongoDatabase;

    public Task Insert<TDocument>(string collectionName, TDocument[] bsonDocuments, CancellationToken cancellationToken)
    {
        return MongoSession.Insert(collectionName, bsonDocuments, cancellationToken);
    }

    public IAsyncEnumerable<TDocument> Find<TDocument>
    (
        string collectionName,
        FilterDefinition<BsonDocument> filterDefinition,
        ProjectionDefinition<BsonDocument, TDocument> projectionDefinition,
        SortDefinition<BsonDocument>? sortDefinition,
        int? skip,
        int? limit,
        MongoDbQueryOptions? options,
        CancellationToken cancellationToken
    )
    {
        return MongoSession.Find
        (
            collectionName,
            filterDefinition,
            projectionDefinition,
            sortDefinition,
            skip,
            limit,
            options,
            cancellationToken
        );
    }

    public Task Delete<TDocument>(string collectionName,
        FilterDefinition<TDocument> filterDefinition, CancellationToken cancellationToken)
    {
        return MongoSession.Delete(collectionName, filterDefinition, cancellationToken);
    }

    public IMongoSession WithTransactionSessionOptions(MongoDbTransactionSessionOptions options)
    {
        return this with { MongoSession = MongoSession.WithTransactionSessionOptions(options) };
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
