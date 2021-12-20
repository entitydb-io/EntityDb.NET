using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Provisioner.Extensions;
using EntityDb.MongoDb.Transactions;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Transactions
{
    internal sealed class
        AutoProvisionMongoDbTransactionRepositoryFactory<TEntity> : MongoDbTransactionRepositoryFactoryWrapper<TEntity>
    {
        private bool _needToProvision = true;

        public AutoProvisionMongoDbTransactionRepositoryFactory(
            IMongoDbTransactionRepositoryFactory<TEntity> mongoDbTransactionRepositoryFactory) : base(
            mongoDbTransactionRepositoryFactory)
        {
        }

        public override async Task<MongoDbTransactionObjects> CreateObjects(
            TransactionSessionOptions transactionSessionOptions)
        {
            var mongoObjects = await base.CreateObjects(transactionSessionOptions);

            if (!_needToProvision)
            {
                return mongoObjects;
            }

            _needToProvision = false;

            await mongoObjects.MongoClient.ProvisionCollections(DatabaseName);

            return mongoObjects;
        }
    }
}
