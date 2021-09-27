using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Entities;
using EntityDb.Common.Loggers;
using EntityDb.Common.Strategies;
using EntityDb.Common.Strategies.Resolving;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace EntityDb.Common.Extensions
{
    /// <summary>
    /// Extensions for service collections.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an internal implementation of <see cref="IResolvingStrategy"/> which resolves types by using assembly information.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public static void AddDefaultResolvingStrategy(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IResolvingStrategy, DefaultResolvingStrategy>();
        }

        /// <summary>
        /// Adds an internal implementation of <see cref="IResolvingStrategy"/> which resolves the given types based on their <see cref="MemberInfo.Name"/>.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="types">The types that can be resolved by <see cref="MemberInfo.Name"/>.</param>
        public static void AddMemberInfoNameResolvingStrategy(this IServiceCollection serviceCollection, Type[] types)
        {
            serviceCollection.AddSingleton<IResolvingStrategy>((serviceProvider) => new MemberInfoNameResolvingStrategy(types));
        }

        /// <summary>
        /// Adds an internal implementation of <see cref="IResolvingStrategyChain"/> to a service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <remarks>
        /// LIFO means Last In, First Out. In other words, the last registered implementation of <see cref="IResolvingStrategy"/> will be the first to attempt to resolve the desired type.
        /// </remarks>
        public static void AddLifoResolvingStrategyChain(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IResolvingStrategyChain, LifoResolvingStrategyChain>();
        }

        /// <summary>
        /// Adds an internal implementation of <see cref="ILoggerFactory"/> to a service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <remarks>
        /// Uses the Microsoft.Extensions.Logging framework under the hood.
        /// </remarks>
        public static void AddDefaultLogger(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();

            serviceCollection.AddSingleton<ILoggerFactory, DefaultLoggerFactory>();
        }

        /// <summary>
        /// Adds a custom implementation of <see cref="IAgentAccessor"/> to a service collection.
        /// </summary>
        /// <typeparam name="TAgentAccessor">The type of the agent accessor.</typeparam>
        /// <param name="serviceCollection">The service collection</param>
        public static void AddAgentAccessor<TAgentAccessor>(this IServiceCollection serviceCollection)
            where TAgentAccessor : class, IAgentAccessor
        {
            serviceCollection.AddScoped<IAgentAccessor, TAgentAccessor>();
        }

        /// <summary>
        /// Adds a custom implementation of <see cref="ISnapshottingStrategy{TEntity}"/> to a service collection.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be snapshotted.</typeparam>
        /// <typeparam name="TSnapshottingStrategy">The type that implements <see cref="ISnapshottingStrategy{TEntity}"/>.</typeparam>
        /// <param name="serviceCollection">The service collection</param>
        public static void AddSnapshottingStrategy<TEntity, TSnapshottingStrategy>(this IServiceCollection serviceCollection)
            where TSnapshottingStrategy : class, ISnapshottingStrategy<TEntity>
        {
            serviceCollection.AddSingleton<ISnapshottingStrategy<TEntity>, TSnapshottingStrategy>();
        }

        /// <summary>
        /// Adds a custom implementation of <see cref="IConstructingStrategy{TEntity}"/> to a service collection.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be constructed.</typeparam>
        /// <typeparam name="TConstructingStrategy">The type that implements <see cref="IConstructingStrategy{TEntity}"/>.</typeparam>
        /// <param name="serviceCollection">The service collection</param>
        public static void AddConstructingStrategy<TEntity, TConstructingStrategy>(this IServiceCollection serviceCollection)
            where TConstructingStrategy : class, IConstructingStrategy<TEntity>
        {
            serviceCollection.AddSingleton<IConstructingStrategy<TEntity>, TConstructingStrategy>();
        }

        /// <summary>
        /// Adds an internal implementation of <see cref="IVersioningStrategy{TEntity}"/> to a service collection for an entity that implements <see cref="IVersionedEntity{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be versioned.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        public static void AddVersionedEntityVersioningStrategy<TEntity>(this IServiceCollection serviceCollection)
            where TEntity : IVersionedEntity<TEntity>
        {
            serviceCollection.AddSingleton<IVersioningStrategy<TEntity>, VersionedEntityVersioningStrategy<TEntity>>();
        }

        /// <summary>
        /// Adds an internal implementation of <see cref="ILeasingStrategy{TEntity}"/> to a service collection for an entity that implements <see cref="ILeasedEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be leased.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        public static void AddLeasedEntityLeasingStrategy<TEntity>(this IServiceCollection serviceCollection)
            where TEntity : ILeasedEntity
        {
            serviceCollection.AddSingleton<ILeasingStrategy<TEntity>, LeasedEntityLeasingStrategy<TEntity>>();
        }

        /// <summary>
        /// Adds an internal implementation of <see cref="IAuthorizingStrategy{TEntity}"/> to a service collection for an entity that implements <see cref="IAuthorizedEntity{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be authorized.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        public static void AddAuthorizedEntityAuthorizingStrategy<TEntity>(this IServiceCollection serviceCollection)
            where TEntity : IAuthorizedEntity<TEntity>
        {
            serviceCollection.AddSingleton<IAuthorizingStrategy<TEntity>, AuthorizedEntityAuthorizingStrategy<TEntity>>();
        }
    }
}
