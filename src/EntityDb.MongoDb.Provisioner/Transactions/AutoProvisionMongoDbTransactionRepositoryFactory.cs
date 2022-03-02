using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Provisioner.Extensions;
using EntityDb.MongoDb.Sessions;
using EntityDb.MongoDb.Transactions;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Transactions;

internal sealed class
    AutoProvisionMongoDbTransactionRepositoryFactory : MongoDbTransactionRepositoryFactoryWrapper
{
    private bool _needToProvision = true;

    public AutoProvisionMongoDbTransactionRepositoryFactory(
        IMongoDbTransactionRepositoryFactory mongoDbTransactionRepositoryFactory) : base(
        mongoDbTransactionRepositoryFactory)
    {
    }

    public override async Task<IMongoSession> CreateSession(TransactionSessionOptions transactionSessionOptions)
    {
        var mongoSession = await base.CreateSession(transactionSessionOptions);

        if (!_needToProvision)
        {
            return mongoSession;
        }

        _needToProvision = false;

        await mongoSession.MongoDatabase.Client.ProvisionCollections(mongoSession.MongoDatabase.DatabaseNamespace.DatabaseName);

        return mongoSession;
    }
}
