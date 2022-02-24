using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Commands;

internal record DeleteDocumentsCommand<TEntity, TTransactionStep>
(
    IMongoSession MongoSession,
    string CollectionName,
    Func<ITransaction<TEntity>, TTransactionStep, FilterDefinition<BsonDocument>?> Build
)
{
    public async Task Execute(ITransaction<TEntity> transaction, TTransactionStep transactionStep)
    {
        var filter = Build.Invoke(transaction, transactionStep);

        if (filter == null)
        {
            return;
        }

        await MongoSession
            .Delete(CollectionName, filter);
    }
}
