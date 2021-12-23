using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Sessions;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal interface IMongoDbTransactionRepositoryFactory<TEntity> : ITransactionRepositoryFactory<TEntity>
    {
        TransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName);

        Task<IMongoSession> CreateSession(TransactionSessionOptions transactionSessionOptions);

        ITransactionRepository<TEntity> CreateRepository(IMongoSession mongoSession);

        async Task<ITransactionRepository<TEntity>> ITransactionRepositoryFactory<TEntity>.CreateRepository(
            string transactionSessionOptionsName)
        {
            var transactionSessionOptions = GetTransactionSessionOptions(transactionSessionOptionsName);

            var mongoSession = await CreateSession(transactionSessionOptions);

            return CreateRepository(mongoSession);
        }
    }
}
