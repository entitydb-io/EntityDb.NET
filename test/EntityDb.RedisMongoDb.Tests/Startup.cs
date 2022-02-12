using EntityDb.Common.Extensions;
using EntityDb.Common.Tests;
using EntityDb.Common.Tests.Implementations.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.RedisMongoDb.Tests
{
    public class Startup : StartupBase
    {
        public override void AddServices(IServiceCollection serviceCollection)
        {
            base.AddServices(serviceCollection);

            // Snapshots

            var redisStartup = new Redis.Tests.Startup();

            redisStartup.AddServices(serviceCollection);

            // Transactions

            var mongoDbStartup = new MongoDb.Tests.Startup();

            mongoDbStartup.AddServices(serviceCollection);

            // Snapshot Transactions

            serviceCollection.AddSnapshotTransactionSubscriber<TransactionEntity>(TestSessionOptions.Write, true);
        }
    }
}
