using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.TypeResolvers;
using EntityDb.Common.Entities;
using EntityDb.Common.Loggers;
using EntityDb.Common.Projections;
using EntityDb.Common.Transactions;
using EntityDb.Common.TypeResolvers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace EntityDb.Common.Extensions
{
    /// <summary>
    ///     Extensions for service collections.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        internal static void Add<TService>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime, Func<IServiceProvider, TService> serviceFactory)
            where TService : class
        {
            serviceCollection.Add(new(typeof(TService), serviceFactory, serviceLifetime));
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
        ///     Adds an internal implementation of <see cref="ILoggerFactory" /> to a service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <remarks>
        ///     Uses the Microsoft.Extensions.Logging framework under the hood.
        /// </remarks>
        public static void AddDefaultLogger(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();

            serviceCollection.AddSingleton<ILoggerFactory, DefaultLoggerFactory>();
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
        ///     Adds an implementation of <see cref="ISnapshotStrategy{TEntity}" /> to a service collection.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TSnapshotStrategy"></typeparam>
        /// <param name="serviceCollection"></param>
        public static void AddSnapshotStrategy<TEntity, TSnapshotStrategy>(
            this IServiceCollection serviceCollection)
            where TSnapshotStrategy : class, ISnapshotStrategy<TEntity>
        {
            serviceCollection
                .AddSingleton<ISnapshotStrategy<TEntity>, TSnapshotStrategy>();
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
        ///     Adds a transient implementation of <see cref="IProjectionRepositoryFactory{TProjection}"/> to a service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TProjection"></typeparam>
        public static void AddProjection<TEntity, TProjection>(this IServiceCollection serviceCollection)
            where TProjection : IProjection<TProjection>
        {
            serviceCollection.AddTransient<IProjectionRepositoryFactory<TProjection>, ProjectionRepositoryFactory<TEntity, TProjection>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TProjection"></typeparam>
        /// <param name="serviceCollection"></param>
        public static void AddSingleEntityProjectingStrategy<TProjection>(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IProjectionStrategy<TProjection>, SingleEntityProjectionStrategy<TProjection>>();
        }    

        /// <summary>
        ///     Adds a transaction subscriber that records snapshots.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="snapshotSessionOptionsName">The agent's intent for the snapshot repository.</param>
        /// <param name="synchronousMode">If <c>true</c> then snapshots will be synchronously recorded.</param>
        public static void AddSnapshotTransactionSubscriber<TEntity>(this IServiceCollection serviceCollection,
            string snapshotSessionOptionsName, bool synchronousMode = false)
        {
            serviceCollection.AddSingleton<ITransactionSubscriber<TEntity>>(serviceProvider =>
                SnapshotTransactionSubscriber<TEntity>.Create(serviceProvider, snapshotSessionOptionsName,
                    synchronousMode));
        }
    }
}
