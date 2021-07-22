using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Extensions;
using EntityDb.Redis.Extensions;
using EntityDb.TestImplementations.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mongo2Go;
using MongoDB.Driver;
using Redis2Go;
using System.Threading.Tasks;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace EntityDb.RedisMongoDb.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();

            serviceCollection.AddSingleton((serviceProvider) => RedisRunner.Start());

            serviceCollection.AddSingleton((serviceProvider) =>
            {
                var mongoDbRunner = MongoDbRunner.Start(singleNodeReplSet: true);

                var provisionTask = Task.Run(async () =>
                {
                    var mongoClient = new MongoClient(mongoDbRunner.ConnectionString);

                    await mongoClient.ProvisionCollections(TransactionEntity.MongoCollectionName);
                });

                provisionTask.Wait();

                return mongoDbRunner;
            });

            serviceCollection.AddDefaultResolvingStrategy();

            serviceCollection.AddLifoResolvingStrategyChain();

            serviceCollection.AddConstructingStrategy<TransactionEntity, TransactionEntityConstructingStrategy>();
            serviceCollection.AddVersionedEntityVersioningStrategy<TransactionEntity>();
            serviceCollection.AddLeasedEntityLeasingStrategy<TransactionEntity>();
            serviceCollection.AddAuthorizedEntityAuthorizingStrategy<TransactionEntity>();

            serviceCollection.AddTestModeRedisSnapshots<TransactionEntity>(TransactionEntity.RedisKeyNamespace, (serviceProvider) =>
            {
                var redisRunner = serviceProvider.GetRequiredService<RedisRunner>();

                return $"127.0.0.1:{redisRunner.Port}";
            });

            serviceCollection.AddTestModeMongoDbTransactions<TransactionEntity>(TransactionEntity.MongoCollectionName, (serviceProvider) =>
            {
                var mongoDbRunner = serviceProvider.GetRequiredService<MongoDbRunner>();

                return mongoDbRunner.ConnectionString;
            });
        }

        public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor testOutputHelperAccessor)
        {
            loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(testOutputHelperAccessor));
        }
    }
}
