using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.DependencyInjection;
using System;
using EntityDb.Common.Agents;

namespace EntityDb.Common.Tests;

public abstract class StartupBase : IStartup
{

    public virtual void AddServices(IServiceCollection serviceCollection)
    {
        // Resolving

        serviceCollection.AddLifoTypeResolver();

        serviceCollection.AddMemberInfoNamePartialTypeResolver(Array.Empty<Type>());

        serviceCollection.AddDefaultPartialTypeResolver();

        // Agent Accessor

        serviceCollection.AddAgentAccessor<UnknownAgentAccessor>();

        // Transaction Entity

        serviceCollection.AddEntity<TestEntity>();

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