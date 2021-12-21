using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Commands
{
    internal record InsertDocumentsCommand<TEntity, TDocument>
    (
        IMongoSession MongoSession,
        string CollectionName,
        Func<ITransaction<TEntity>, int, ILogger, IReadOnlyCollection<TDocument>?> Build
    )
    {
        public async Task Execute(ITransaction<TEntity> transaction, int transactionStepIndex)
        {
            var documents = Build.Invoke(transaction, transactionStepIndex, MongoSession.Logger);

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
}
