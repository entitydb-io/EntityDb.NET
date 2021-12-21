using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Commands
{
    internal record DeleteDocumentsCommand<TEntity>
    (
        IMongoSession MongoSession,
        string CollectionName,
        Func<ITransaction<TEntity>, int, FilterDefinition<BsonDocument>?> Build
    )
    {
        public async Task Execute(ITransaction<TEntity> transaction, int transactionStepindex)
        {
            var filter = Build.Invoke(transaction, transactionStepindex);

            if (filter == null)
            {
                return;
            }

            await MongoSession
                .Delete(CollectionName, filter);
        }
    }
}
