using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Sessions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions;

internal interface IMongoDbTransactionRepositoryFactory : ITransactionRepositoryFactory
{
    async Task<ITransactionRepository> ITransactionRepositoryFactory.CreateRepository(
        string transactionSessionOptionsName, CancellationToken cancellationToken)
    {
        var options = GetTransactionSessionOptions(transactionSessionOptionsName);

        var mongoSession = await CreateSession(options, cancellationToken);

        return CreateRepository(mongoSession);
    }

    MongoTransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName);

    Task<IMongoSession> CreateSession(MongoTransactionSessionOptions options,
        CancellationToken cancellationToken);

    ITransactionRepository CreateRepository(IMongoSession mongoSession);
}
