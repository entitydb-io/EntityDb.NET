using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Sessions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions;

internal interface IMongoDbTransactionRepositoryFactory : ITransactionRepositoryFactory
{
    async Task<ITransactionRepository> ITransactionRepositoryFactory.CreateRepository(
        string transactionSessionOptionsName, CancellationToken cancellationToken)
    {
        var transactionSessionOptions = GetTransactionSessionOptions(transactionSessionOptionsName);

        var mongoSession = await CreateSession(transactionSessionOptions, cancellationToken);

        return CreateRepository(mongoSession);
    }

    TransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName);

    Task<IMongoSession> CreateSession(TransactionSessionOptions transactionSessionOptions,
        CancellationToken cancellationToken);

    ITransactionRepository CreateRepository(IMongoSession mongoSession);
}
