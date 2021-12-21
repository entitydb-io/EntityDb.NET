using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Transactions;
using EntityDb.TestImplementations.Agents;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Tests
{
    public abstract class StartupBase : IStartup
    {
        public virtual void AddServices(IServiceCollection serviceCollection)
        {
            // Logging

            serviceCollection.AddDefaultLogger();

            // Resolving

            serviceCollection.AddLifoResolvingStrategyChain();

            serviceCollection.AddDefaultResolvingStrategy();

            // Agent

            serviceCollection.AddAgentAccessor<DummyAgentAccessor>();

            // Transaction Entity

            serviceCollection.AddEntity<TransactionEntity, TransactionEntityConstructingStrategy>();

            serviceCollection.AddLeasedEntityLeasingStrategy<TransactionEntity>();

            serviceCollection.AddTaggedEntityTaggingStrategy<TransactionEntity>();

            serviceCollection.AddAuthorizedEntityAuthorizingStrategy<TransactionEntity>();

            // Snapshot Session Options

            serviceCollection.Configure<SnapshotSessionOptions>("TestWrite", _ =>
            {
            });

            // Transaction Session Options

            serviceCollection.Configure<TransactionSessionOptions>("TestWrite", options =>
            {
                options.ReadOnly = false;
            });

            serviceCollection.Configure<TransactionSessionOptions>("TestReadOnly", options =>
            {
                options.ReadOnly = true;
            });
        }
    }
}
