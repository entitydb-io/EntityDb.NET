using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Builders;
using EntityDb.Common.Entities;
using EntityDb.Common.Projections;
using EntityDb.Common.Transactions.Builders;
using EntityDb.Common.Transactions.Subscribers;
using EntityDb.Common.Transactions.Subscribers.ProcessorQueues;
using EntityDb.Common.Transactions.Subscribers.Processors;
using EntityDb.Common.TypeResolvers;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks.Dataflow;

namespace EntityDb.Common.Extensions;

/// <summary>
///     Extensions for service collections.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static void AddTestModeTransactionProcessorQueue<TTransactionProcessor>(this IServiceCollection serviceCollection)
        where TTransactionProcessor : ITransactionProcessor
    {
        serviceCollection.AddSingleton<ITransactionProcessorQueue<TTransactionProcessor>, TestModeTransactionQueue<TTransactionProcessor>>();
    }

    [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
    private static void AddBufferBlockTransactionProcessorQueue<TTransactionProcessor>(this IServiceCollection serviceCollection)
        where TTransactionProcessor : ITransactionProcessor
    {
        serviceCollection.AddSingleton<BufferBlockTransactionQueue<TTransactionProcessor>>();

        serviceCollection.AddSingleton<ITransactionProcessorQueue<TTransactionProcessor>>(serviceProvider => serviceProvider.GetRequiredService<BufferBlockTransactionQueue<TTransactionProcessor>>());

        serviceCollection.AddHostedService(serviceProvider =>
          serviceProvider.GetRequiredService<BufferBlockTransactionQueue<TTransactionProcessor>>());
    }

    internal static void Add<TService>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime,
        Func<IServiceProvider, TService> serviceFactory)
        where TService : class
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(TService), serviceFactory, serviceLifetime));
    }

    internal static void Add<TService, TImplementation>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime)
        where TService : class
        where TImplementation : TService
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), serviceLifetime));
    }

    internal static void Add<TService>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime)
        where TService : class
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(TService), typeof(TService), serviceLifetime));
    }

    /// <summary>
    ///     Registers the transaction processor provided, along with a transaction processor subscriber,
    ///     and a transaction processor queue. For test mode, the queue is not actually a queue and will
    ///     immediately process the transaction. For non-test mode, the queue uses a <see cref="BufferBlock{ITransaction}"/>.
    /// </summary>
    /// <typeparam name="TTransactionProcessor">The type of the transaction processor.</typeparam>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="testMode">Wether or not to run in test mode.</param>
    /// <param name="transactionProcessorFactory">A factory for creating the transaction processor.</param>
    [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
    public static void AddTransactionProcessorSubscriber<TTransactionProcessor>(this IServiceCollection serviceCollection,
        bool testMode, Func<IServiceProvider, TTransactionProcessor> transactionProcessorFactory)
        where TTransactionProcessor : class, ITransactionProcessor
    {
        serviceCollection.AddSingleton(transactionProcessorFactory.Invoke);

        serviceCollection.AddSingleton<TransactionProcessorSubscriber<TTransactionProcessor>>();

        serviceCollection.AddSingleton<ITransactionSubscriber, TransactionProcessorSubscriber<TTransactionProcessor>>();

        if (testMode)
        {
            serviceCollection.AddTestModeTransactionProcessorQueue<TTransactionProcessor>();
        }
        else
        {
            serviceCollection.AddBufferBlockTransactionProcessorQueue<TTransactionProcessor>();
        }
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
    ///     Adds an internal implementation of <see cref="IPartialTypeResolver" /> which resolves the given types based on
    ///     their
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
    ///     Adds a transient <see cref="ITransactionBuilder{TEntity}" /> and a transient implementation of
    ///     <see cref="IEntityRepositoryFactory{TEntity}" /> to a service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public static void AddEntity<TEntity>(this IServiceCollection serviceCollection)
        where TEntity : IEntity<TEntity>
    {
        serviceCollection.AddTransient<ITransactionBuilderFactory<TEntity>, TransactionBuilderFactory<TEntity>>();

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
        serviceCollection.AddTransactionProcessorSubscriber(testMode, serviceProvider => EntitySnapshotTransactionProcessor<TEntity>.Create(
            serviceProvider, transactionSessionOptionsName, snapshotSessionOptionsName));
    }

    /// <summary>
    ///     Adds projections for <typeparamref name="TProjection" />.
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
        serviceCollection.AddTransactionProcessorSubscriber(testMode, serviceProvider => ProjectionSnapshotTransactionProcessor<TProjection>.Create(
            serviceProvider, transactionSessionOptionsName, snapshotSessionOptionsName));
    }
}
