using EntityDb.Common.Extensions;
using EntityDb.TestImplementations.Agents;
using EntityDb.TestImplementations.Entities;
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

            serviceCollection.AddConstructingStrategy<TransactionEntity, TransactionEntityConstructingStrategy>();
            serviceCollection.AddVersionedEntityVersioningStrategy<TransactionEntity>();
            serviceCollection.AddLeasedEntityLeasingStrategy<TransactionEntity>();
            serviceCollection.AddAuthorizedEntityAuthorizingStrategy<TransactionEntity>();
        }

        public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor testOutputHelperAccessor)
        {
            loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(testOutputHelperAccessor));
        }
    }
}
