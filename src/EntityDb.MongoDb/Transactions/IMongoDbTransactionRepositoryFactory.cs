using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Sessions;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions;

internal interface IMongoDbTransactionRepositoryFactory : ITransactionRepositoryFactory
{
    TransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName);

    Task<IMongoSession> CreateSession(TransactionSessionOptions transactionSessionOptions);

    ITransactionRepository CreateRepository(IMongoSession mongoSession);

    async Task<ITransactionRepository> ITransactionRepositoryFactory.CreateRepository(
        string transactionSessionOptionsName)
    {
        var transactionSessionOptions = GetTransactionSessionOptions(transactionSessionOptionsName);

        var mongoSession = await CreateSession(transactionSessionOptions);

        return CreateRepository(mongoSession);
    }
}
