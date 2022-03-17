using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Provisioner.Extensions;
using EntityDb.MongoDb.Sessions;
using EntityDb.MongoDb.Transactions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Transactions;

internal sealed class
    AutoProvisionMongoDbTransactionRepositoryFactory : MongoDbTransactionRepositoryFactoryWrapper
{
    private static readonly SemaphoreSlim Lock = new(1);
    private static bool _provisioned;

    public AutoProvisionMongoDbTransactionRepositoryFactory(
        IMongoDbTransactionRepositoryFactory mongoDbTransactionRepositoryFactory) : base(
        mongoDbTransactionRepositoryFactory)
    {
    }

    public override async Task<IMongoSession> CreateSession(TransactionSessionOptions transactionSessionOptions, CancellationToken cancellationToken)
    {
        var mongoSession = await base.CreateSession(transactionSessionOptions, cancellationToken);

        await Lock.WaitAsync(cancellationToken);

        if (_provisioned)
        {
            return mongoSession;
        }
        
        await mongoSession.MongoDatabase.Client.ProvisionCollections(mongoSession.MongoDatabase.DatabaseNamespace
            .DatabaseName, cancellationToken);
        
        _provisioned = true;
        
        Lock.Release();

        return mongoSession;
    }
}
