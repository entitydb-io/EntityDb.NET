using EntityDb.Abstractions.Disposables;
using EntityDb.Common.Transactions;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions;

internal interface IMongoSession : IDisposableResource
{
    IMongoDatabase MongoDatabase { get; }

    Task Insert<TDocument>(string collectionName,
        TDocument[] bsonDocuments, CancellationToken cancellationToken);

    Task<List<TDocument>> Find<TDocument>
    (
        string collectionName,
        FilterDefinition<BsonDocument> filterDefinition,
        ProjectionDefinition<BsonDocument, TDocument> projectionDefinition,
        SortDefinition<BsonDocument>? sortDefinition,
        int? skip,
        int? limit,
        CancellationToken cancellationToken
    );

    Task Delete<TDocument>(string collectionName,
        FilterDefinition<TDocument> filterDefinition, CancellationToken cancellationToken);

    void StartTransaction();
    Task CommitTransaction(CancellationToken cancellationToken);
    Task AbortTransaction();

    IMongoSession WithTransactionSessionOptions(TransactionSessionOptions transactionSessionOptions);
}
