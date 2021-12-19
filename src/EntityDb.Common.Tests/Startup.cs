using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Transactions;
using EntityDb.TestImplementations.Agents;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace EntityDb.Common.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDefaultLogger();

            serviceCollection.AddDefaultResolvingStrategy();

            // This is only here to add coverage
            serviceCollection.AddMemberInfoNameResolvingStrategy(new[] { typeof(object) });

            serviceCollection.AddLifoResolvingStrategyChain();

            serviceCollection.AddAgentAccessor<DummyAgentAccessor>();

            serviceCollection.AddEntity<TransactionEntity, TransactionEntityConstructingStrategy>();

            serviceCollection.AddLeasedEntityLeasingStrategy<TransactionEntity>();
            serviceCollection.AddAuthorizedEntityAuthorizingStrategy<TransactionEntity>();
        }

        public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor testOutputHelperAccessor)
        {
            loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(testOutputHelperAccessor));
        }

        public static void ConfigureTestsBaseServices(IServiceCollection serviceCollection)
        {
            serviceCollection.Configure<SnapshotSessionOptions>("TestWrite", (options) =>
            {
                options.TestMode = true;
            });

            serviceCollection.Configure<TransactionSessionOptions>("TestWrite", (options) =>
            {
                options.TestMode = true;
            });

            serviceCollection.Configure<TransactionSessionOptions>("TestReadOnly", (options) =>
            {
                options.TestMode = true;
                options.ReadOnly = true;
            });
        }
    }
}
