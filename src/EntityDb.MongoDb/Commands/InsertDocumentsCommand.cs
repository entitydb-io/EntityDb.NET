using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Commands;

internal record InsertDocumentsCommand<TTransactionStep, TDocument>
(
    IMongoSession MongoSession,
    string CollectionName,
    Func<ITransaction, TTransactionStep, ILogger, IReadOnlyCollection<TDocument>?> Build
)
{
    public async Task Execute(ITransaction transaction, TTransactionStep transactionStep)
    {
        var documents = Build.Invoke(transaction, transactionStep, MongoSession.Logger);

        if (documents == null)
        {
            return;
        }

        var bsonDocuments = documents
            .Select(document => document.ToBsonDocument())
            .ToArray();

        await MongoSession
            .Insert(CollectionName, bsonDocuments);
    }
}
