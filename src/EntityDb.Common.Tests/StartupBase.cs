using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Transactions;
using EntityDb.TestImplementations.Agents;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Strategies;
using Microsoft.Extensions.DependencyInjection;
using System;

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

            serviceCollection.AddMemberInfoNameResolvingStrategy(Array.Empty<Type>());

            serviceCollection.AddDefaultResolvingStrategy();

            // Agent

            serviceCollection.AddAgentAccessor<NoAgentAccessor>();

            // Transaction Entity

            serviceCollection.AddEntity<TransactionEntity, TransactionEntityConstructingStrategy>();

            serviceCollection.AddLeasedEntityLeasingStrategy<TransactionEntity>();

            serviceCollection.AddTaggedEntityTaggingStrategy<TransactionEntity>();

            serviceCollection.AddAuthorizedEntityAuthorizingStrategy<TransactionEntity>();

            // Snapshot Session Options

            serviceCollection.Configure<SnapshotSessionOptions>(TestSessionOptions.Write, options =>
            {
                options.ReadOnly = false;
            });

            serviceCollection.Configure<SnapshotSessionOptions>(TestSessionOptions.ReadOnly, options =>
            {
                options.ReadOnly = true;
                options.SecondaryPreferred = false;
            });

            serviceCollection.Configure<SnapshotSessionOptions>(TestSessionOptions.ReadOnlySecondaryPreferred, options =>
            {
                options.ReadOnly = true;
                options.SecondaryPreferred = true;
            });

            // Transaction Session Options

            serviceCollection.Configure<TransactionSessionOptions>(TestSessionOptions.Write, options =>
            {
                options.ReadOnly = false;
                options.WriteTimeout = TimeSpan.FromSeconds(1);
            });

            serviceCollection.Configure<TransactionSessionOptions>(TestSessionOptions.ReadOnly, options =>
            {
                options.ReadOnly = true;
                options.SecondaryPreferred = false;
                options.ReadTimeout = TimeSpan.FromSeconds(1);
            });

            serviceCollection.Configure<TransactionSessionOptions>(TestSessionOptions.ReadOnlySecondaryPreferred, options =>
            {
                options.ReadOnly = true;
                options.SecondaryPreferred = true;
                options.ReadTimeout = TimeSpan.FromSeconds(1);
            });
        }
    }
}
