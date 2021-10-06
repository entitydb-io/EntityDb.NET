using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Provisioner.Extensions;
using EntityDb.Redis.Extensions;
using EntityDb.TestImplementations.Agents;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace EntityDb.RedisMongoDb.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDefaultLogger();

            serviceCollection.AddAgentAccessor<DummyAgentAccessor>();

            serviceCollection.AddDefaultResolvingStrategy();

            serviceCollection.AddLifoResolvingStrategyChain();

            serviceCollection.AddEntity<TransactionEntity, TransactionEntityConstructingStrategy>();
            serviceCollection.AddLeasedEntityLeasingStrategy<TransactionEntity>();
            serviceCollection.AddAuthorizedEntityAuthorizingStrategy<TransactionEntity>();

            serviceCollection.AddTestModeRedisSnapshots<TransactionEntity>
            (
                TransactionEntity.RedisKeyNamespace,
                _ => "127.0.0.1:6379"
            );

            serviceCollection.AddAutoProvisionTestModeMongoDbTransactions<TransactionEntity>
            (
                TransactionEntity.MongoCollectionName,
                _ => "mongodb://127.0.0.1:27017/?connect=direct&replicaSet=entitydb"
            );
        }

        public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor testOutputHelperAccessor)
        {
            loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(testOutputHelperAccessor));
        }
    }
}
