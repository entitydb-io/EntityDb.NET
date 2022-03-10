using EntityDb.Common.Disposables;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions;

internal record TestModeMongoSession(IMongoSession MongoSession) : DisposableResourceBaseRecord, IMongoSession
{
    public IMongoDatabase MongoDatabase => MongoSession.MongoDatabase;
    public IClientSessionHandle ClientSessionHandle => MongoSession.ClientSessionHandle;

    public Task Insert<TDocument>(string collectionName, TDocument[] bsonDocuments)
    {
        return MongoSession.Insert(collectionName, bsonDocuments);
    }

    public Task<List<TDocument>> Find<TDocument>
    (
        string collectionName,
        FilterDefinition<BsonDocument> filterDefinition,
        ProjectionDefinition<BsonDocument, TDocument> projectionDefinition,
        SortDefinition<BsonDocument>? sortDefinition,
        int? skip,
        int? limit
    )
    {
        return MongoSession.Find(collectionName, filterDefinition, projectionDefinition, sortDefinition, skip, limit);
    }

    public Task Delete<TDocument>(string collectionName,
        FilterDefinition<TDocument> filterDefinition)
    {
        return MongoSession.Delete(collectionName, filterDefinition);
    }

    public IMongoSession WithTransactionSessionOptions(TransactionSessionOptions transactionSessionOptions)
    {
        return this with
        {
            MongoSession = MongoSession.WithTransactionSessionOptions(transactionSessionOptions)
        };
    }

    public void StartTransaction()
    {
        // Test Mode Transactions are started in the Test Mode Repository Factory
    }

    public Task CommitTransaction()
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
