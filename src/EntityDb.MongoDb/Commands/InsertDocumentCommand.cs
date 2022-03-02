using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Commands;

internal record InsertDocumentCommand<TDocument>
(
    IMongoSession MongoSession,
    string CollectionName,
    Func<ITransaction, ILogger, TDocument> Build
)
{
    public async Task Execute(ITransaction transaction)
    {
        var document = Build.Invoke(transaction, MongoSession.Logger);

        var bsonDocument = document.ToBsonDocument();

        await MongoSession
            .Insert(CollectionName, new[] { bsonDocument });
    }
}
