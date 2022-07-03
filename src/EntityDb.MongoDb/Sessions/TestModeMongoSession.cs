using EntityDb.Common.Disposables;
using EntityDb.Common.Transactions;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions;

internal record TestModeMongoSession(IMongoSession MongoSession) : DisposableResourceBaseRecord, IMongoSession
{
    public IMongoDatabase MongoDatabase => MongoSession.MongoDatabase;

    public Task Insert<TDocument>(string collectionName, TDocument[] bsonDocuments, CancellationToken cancellationToken)
    {
        return MongoSession.Insert(collectionName, bsonDocuments, cancellationToken);
    }

    public Task<List<TDocument>> Find<TDocument>
    (
        string collectionName,
        FilterDefinition<BsonDocument> filterDefinition,
        ProjectionDefinition<BsonDocument, TDocument> projectionDefinition,
        SortDefinition<BsonDocument>? sortDefinition,
        int? skip,
        int? limit,
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
            cancellationToken
        );
    }

    public Task Delete<TDocument>(string collectionName,
        FilterDefinition<TDocument> filterDefinition, CancellationToken cancellationToken)
    {
        return MongoSession.Delete(collectionName, filterDefinition, cancellationToken);
    }

    public IMongoSession WithTransactionSessionOptions(TransactionSessionOptions transactionSessionOptions)
    {
        return this with { MongoSession = MongoSession.WithTransactionSessionOptions(transactionSessionOptions) };
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
