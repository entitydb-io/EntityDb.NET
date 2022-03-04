using EntityDb.Abstractions.Disposables;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions;

internal interface IMongoSession : IDisposableResource
{
    ILogger<IMongoSession> Logger { get; }
    IMongoDatabase MongoDatabase { get; }
    IClientSessionHandle ClientSessionHandle { get; }

    Task Insert<TDocument>(string collectionName,
        TDocument[] bsonDocuments);
    Task<List<TDocument>> Find<TDocument>
    (
        string collectionName,
        FilterDefinition<BsonDocument> filterDefinition,
        ProjectionDefinition<BsonDocument, TDocument> projectionDefinition,
        SortDefinition<BsonDocument>? sortDefinition,
        int? skip,
        int? limit
    );
        
    Task Delete<TDocument>(string collectionName,
        FilterDefinition<TDocument> filterDefinition);

    void StartTransaction();
    Task CommitTransaction();
    Task AbortTransaction();

    IMongoSession WithTransactionSessionOptions(TransactionSessionOptions transactionSessionOptions);
}
