using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Sessions;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal interface IMongoDbTransactionRepositoryFactory<TEntity> : ITransactionRepositoryFactory<TEntity>
    {
        string DatabaseName { get; }
        IMongoClient CreatePrimaryClient();
        IMongoClient CreateSecondaryClient();
        Task<IClientSessionHandle> CreateClientSessionHandle();
        TransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName);
        ITransactionRepository<TEntity> CreateRepository(TransactionSessionOptions transactionSessionOptions, IMongoSession? mongoSession, IMongoClient mongoClient);
        Task<MongoDbTransactionObjects> CreateObjects(TransactionSessionOptions transactionSessionOptions);

        async Task<ITransactionRepository<TEntity>> ITransactionRepositoryFactory<TEntity>.CreateRepository(string transactionSessionOptionsName)
        {
            var transactionSessionOptions = GetTransactionSessionOptions(transactionSessionOptionsName);
            var (mongoSession, mongoClient) = await CreateObjects(transactionSessionOptions);

            return CreateRepository(transactionSessionOptions, mongoSession, mongoClient);
        }
    }
}
