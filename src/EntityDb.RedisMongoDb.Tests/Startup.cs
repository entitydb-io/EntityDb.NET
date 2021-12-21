using EntityDb.Common.Extensions;
using EntityDb.Common.Tests;
using EntityDb.MongoDb.Provisioner.Extensions;
using EntityDb.Redis.Extensions;
using EntityDb.TestImplementations.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.RedisMongoDb.Tests
{
    public class Startup : StartupBase
    {
        public override void AddServices(IServiceCollection serviceCollection)
        {
            base.AddServices(serviceCollection);

            // Snapshots

            serviceCollection.AddRedisSnapshots<TransactionEntity>
            (
                TransactionEntity.RedisKeyNamespace,
                _ => "127.0.0.1:6379",
                true
            );

            // Transactions

            serviceCollection.AddAutoProvisionTestModeMongoDbTransactions<TransactionEntity>
            (
                TransactionEntity.MongoCollectionName,
                _ => "mongodb://127.0.0.1:27017/?connect=direct&replicaSet=entitydb",
                true
            );

            // Snapshot Transactions

            serviceCollection.AddSnapshotTransactionSubscriber<TransactionEntity>("TestWrite", true);
        }
    }
}
