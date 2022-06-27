using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Entities;
using EntityDb.Common.Projections;
using EntityDb.Common.Transactions.Builders;
using EntityDb.Common.Transactions.Processors;
using EntityDb.Common.Transactions.Subscribers;
using EntityDb.Common.TypeResolvers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace EntityDb.Common.Extensions;

/// <summary>
///     Extensions for service collections.
/// </summary>
public static class ServiceCollectionExtensions
{
    internal static void Add<TService>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime, Func<IServiceProvider, TService> serviceFactory)
        where TService : class
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(TService), serviceFactory, serviceLifetime));
    }

    /// <summary>
    ///     Adds an internal implementation of <see cref="IPartialTypeResolver" /> which resolves types by using assembly
    ///     information.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    public static void AddDefaultPartialTypeResolver(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IPartialTypeResolver, DefaultPartialTypeResolver>();
    }

    /// <summary>
    ///     Adds an internal implementation of <see cref="IPartialTypeResolver" /> which resolves the given types based on their
    ///     <see cref="MemberInfo.Name" />.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="types">The types that can be resolved by <see cref="MemberInfo.Name" />.</param>
    public static void AddMemberInfoNamePartialTypeResolver(this IServiceCollection serviceCollection, Type[] types)
    {
        serviceCollection.AddSingleton<IPartialTypeResolver>(_ =>
            new MemberInfoNamePartialTypeResolver(types));
    }

    /// <summary>
    ///     Adds an internal implementation of <see cref="ITypeResolver" /> to a service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <remarks>
    ///     LIFO means Last In, First Out. In other words, the last registered implementation of
    ///     <see cref="IPartialTypeResolver" /> will be the first to attempt to resolve the desired type.
    /// </remarks>
    public static void AddLifoTypeResolver(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ITypeResolver, LifoTypeResolver>();
    }

    /// <summary>
    ///     Adds a custom implementation of <see cref="IAgentAccessor" /> to a service collection.
    /// </summary>
    /// <typeparam name="TAgentAccessor">The type of the agent accessor.</typeparam>
    /// <param name="serviceCollection">The service collection</param>
    public static void AddAgentAccessor<TAgentAccessor>(this IServiceCollection serviceCollection)
        where TAgentAccessor : class, IAgentAccessor
    {
        serviceCollection.AddSingleton<IAgentAccessor, TAgentAccessor>();
    }

    /// <summary>
    ///     Adds a transient <see cref="TransactionBuilder{TEntity}"/> and a transient implementation of <see cref="IEntityRepositoryFactory{TEntity}"/> to a service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public static void AddEntity<TEntity>(this IServiceCollection serviceCollection)
        where TEntity : IEntity<TEntity>
    {
        serviceCollection.AddTransient<TransactionBuilder<TEntity>>();

        serviceCollection.AddTransient<IEntityRepositoryFactory<TEntity>, EntityRepositoryFactory<TEntity>>();
    }

    /// <summary>
    ///     Adds a transaction subscriber that records snapshots of entities.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="transactionSessionOptionsName">The agent's intent for the transaction repository.</param>
    /// <param name="snapshotSessionOptionsName">The agent's intent for the snapshot repository.</param>
    /// <param name="testMode">If <c>true</c> then snapshots will be synchronously recorded.</param>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public static void AddEntitySnapshotTransactionSubscriber<TEntity>(this IServiceCollection serviceCollection,
        string transactionSessionOptionsName, string snapshotSessionOptionsName, bool testMode = false)
        where TEntity : IEntity<TEntity>
    {
        serviceCollection.AddSingleton
        (
            serviceProvider =>
            {
                var transactionProcessor = EntitySnapshotTransactionProcessor<TEntity>.Create(
                    serviceProvider, transactionSessionOptionsName, snapshotSessionOptionsName);

                return TransactionProcessorSubscriber<EntitySnapshotTransactionProcessor<TEntity>>.Create(
                    serviceProvider, transactionProcessor, testMode);
            }
        );

        serviceCollection.AddSingleton<ITransactionSubscriber>(
            serviceProvider => serviceProvider.GetRequiredService<TransactionProcessorSubscriber<EntitySnapshotTransactionProcessor<TEntity>>>()
        );

        if (testMode)
        {
            return;
        }

        serviceCollection.AddHostedService(
            serviceProvider => serviceProvider.GetRequiredService<TransactionProcessorSubscriber<EntitySnapshotTransactionProcessor<TEntity>>>()
        );
    }

    /// <summary>
    ///     Adds projections for <typeparamref name="TProjection"/>.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <typeparam name="TProjection">The type of the projection.</typeparam>
    public static void AddProjection<TProjection>(
        this IServiceCollection serviceCollection)
        where TProjection : IProjection<TProjection>
    {
        serviceCollection
            .AddTransient<IProjectionRepositoryFactory<TProjection>, ProjectionRepositoryFactory<TProjection>>();
    }

    /// <summary>
    ///     Adds a transaction subscriber that records snapshots of projections.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="transactionSessionOptionsName">The agent's intent for the transaction repository.</param>
    /// <param name="snapshotSessionOptionsName">The agent's intent for the snapshot repository.</param>
    /// <param name="testMode">If <c>true</c> then snapshots will be synchronously recorded.</param>
    /// <typeparam name="TProjection">The type of the projection.</typeparam>
    public static void AddProjectionSnapshotTransactionSubscriber<TProjection>(
        this IServiceCollection serviceCollection,
        string transactionSessionOptionsName, string snapshotSessionOptionsName, bool testMode = false)
        where TProjection : IProjection<TProjection>
    {
        serviceCollection.AddSingleton
        (
            serviceProvider =>
            {
                var transactionProcessor = ProjectionSnapshotTransactionProcessor<TProjection>.Create(
                    serviceProvider, transactionSessionOptionsName, snapshotSessionOptionsName);

                return TransactionProcessorSubscriber<ProjectionSnapshotTransactionProcessor<TProjection>>.Create(
                    serviceProvider, transactionProcessor, testMode);
            }
        );

        serviceCollection.AddSingleton<ITransactionSubscriber>(
            serviceProvider => serviceProvider.GetRequiredService<TransactionProcessorSubscriber<ProjectionSnapshotTransactionProcessor<TProjection>>>()
        );

        if (testMode)
        {
            return;
        }

        serviceCollection.AddHostedService(
            serviceProvider => serviceProvider.GetRequiredService<TransactionProcessorSubscriber<ProjectionSnapshotTransactionProcessor<TProjection>>>()
        );
    }
}
