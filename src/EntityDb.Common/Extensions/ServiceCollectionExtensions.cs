using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Entities;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Transactions;
using EntityDb.Common.Transactions.Builders;
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
        serviceCollection.AddScoped<IAgentAccessor, TAgentAccessor>();
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
    ///     Adds a transaction subscriber that records snapshots.
    /// </summary>
    /// <typeparam name="TSnapshot">The type of the snapshot.</typeparam>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="snapshotSessionOptionsName">The agent's intent for the snapshot repository.</param>
    /// <param name="synchronousMode">If <c>true</c> then snapshots will be synchronously recorded.</param>
    public static void AddSnapshotTransactionSubscriber<TSnapshot>(this IServiceCollection serviceCollection,
        string snapshotSessionOptionsName, bool synchronousMode = false)
        where TSnapshot : ISnapshot<TSnapshot>
    {
        serviceCollection.AddSingleton<ITransactionSubscriber>(serviceProvider =>
            SnapshotTransactionSubscriber<TSnapshot>.Create(serviceProvider, snapshotSessionOptionsName,
                synchronousMode));
    }
}
