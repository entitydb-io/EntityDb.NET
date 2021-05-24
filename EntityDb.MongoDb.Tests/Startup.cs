using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Extensions;
using EntityDb.TestImplementations.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mongo2Go;
using MongoDB.Driver;
using System.Threading.Tasks;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace EntityDb.MongoDb.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();

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
            serviceCollection.AddTaggedEntityTaggingStrategy<TransactionEntity>();
            serviceCollection.AddAuthorizedEntityAuthorizingStrategy<TransactionEntity>();

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
