using EntityDb.Abstractions.Disposables;
using EntityDb.MongoDb.Queries;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Transactions.Sessions;

internal interface IMongoSession : IDisposableResource
{
    IMongoDatabase MongoDatabase { get; }

    Task Insert<TDocument>(string collectionName,
        TDocument[] bsonDocuments, CancellationToken cancellationToken);

    IAsyncEnumerable<TDocument> Find<TDocument>
    (
        string collectionName,
        FilterDefinition<BsonDocument> filterDefinition,
        ProjectionDefinition<BsonDocument, TDocument> projectionDefinition,
        SortDefinition<BsonDocument>? sortDefinition,
        int? skip,
        int? limit,
        MongoDbQueryOptions? options,
        CancellationToken cancellationToken
    );

    Task Delete<TDocument>(string collectionName,
        FilterDefinition<TDocument> filterDefinition, CancellationToken cancellationToken);

    void StartTransaction();
    Task CommitTransaction(CancellationToken cancellationToken);
    Task AbortTransaction();

    IMongoSession WithTransactionSessionOptions(MongoDbTransactionSessionOptions options);
}
