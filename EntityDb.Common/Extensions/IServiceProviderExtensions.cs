using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EntityDb.Common.Extensions
{
    /// <summary>
    /// Extensions for service providers.
    /// </summary>
    public static class IServiceProviderExtensions
    {
        /// <summary>
        /// Returns the resolved <see cref="Type"/> or throws if the <see cref="Type"/> cannot be resolved.
        /// </summary>
        /// <param name="serviceProvider">The servie provider.</param>
        /// <param name="assemblyFullName">The <see cref="Assembly.FullName"/> of the <see cref="Type.Assembly"/>.</param>
        /// <param name="typeFullName">The <see cref="Type.FullName"/>.</param>
        /// <param name="typeName">The <see cref="MemberInfo.Name"/>.</param>
        /// <returns>The resolved <see cref="Type"/>.</returns>
        public static Type ResolveType(this IServiceProvider serviceProvider, string? assemblyFullName, string? typeFullName, string? typeName)
        {
            var resolvingStrategyChain = serviceProvider.GetRequiredService<IResolvingStrategyChain>();

            return resolvingStrategyChain.ResolveType(assemblyFullName, typeFullName, typeName);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ITransactionRepository{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="transactionSessionOptions">The agent's use case for the repository.</param>
        /// <returns>A new instance of <see cref="ITransactionRepository{TEntity}"/>.</returns>
        public static Task<ITransactionRepository<TEntity>> CreateTransactionRepository<TEntity>(this IServiceProvider serviceProvider, ITransactionSessionOptions transactionSessionOptions)
        {
            var transactionRepositoryFactory = serviceProvider.GetRequiredService<ITransactionRepositoryFactory<TEntity>>();

            return transactionRepositoryFactory.CreateRepository(transactionSessionOptions);
        }

        /// <summary>
        /// Create a new instance of <see cref="ISnapshotRepository{TEntity}"/>
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="snapshotSessionOptions">The agent's use case for the repository.</param>
        /// <returns>A new instance of <see cref="ISnapshotRepository{TEntity}"/>.</returns>
        public static async Task<ISnapshotRepository<TEntity>> CreateSnapshotRepository<TEntity>(this IServiceProvider serviceProvider, ISnapshotSessionOptions snapshotSessionOptions)
        {
            var snapshotRepositoryFactory = serviceProvider.GetRequiredService<ISnapshotRepositoryFactory<TEntity>>();

            return await snapshotRepositoryFactory.CreateRepository(snapshotSessionOptions);
        }

        /// <summary>
        /// Returns a new instance of a <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="entityId">The id of the entity.</param>
        /// <returns>A new instance of <typeparamref name="TEntity"/>.</returns>
        public static TEntity Construct<TEntity>(this IServiceProvider serviceProvider, Guid entityId)
        {
            var constructingStrategy = serviceProvider.GetRequiredService<IConstructingStrategy<TEntity>>();

            return constructingStrategy.Construct(entityId);
        }

        /// <summary>
        /// Returns the version number of a <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The version number of <paramref name="entity"/>.</returns>
        public static ulong GetVersionNumber<TEntity>(this IServiceProvider serviceProvider, TEntity entity)
        {
            var versioningStrategy = serviceProvider.GetRequiredService<IVersioningStrategy<TEntity>>();

            return versioningStrategy.GetVersionNumber(entity);
        }

        /// <summary>
        /// Creates a new version number modifier for an entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="versionNumber">The desired version number.</param>
        /// <returns>A new version number modifier for on an entity.</returns>
        public static IFact<TEntity> GetVersionNumberFact<TEntity>(this IServiceProvider serviceProvider, ulong versionNumber)
        {
            var versioningStrategy = serviceProvider.GetRequiredService<IVersioningStrategy<TEntity>>();

            return versioningStrategy.GetVersionNumberFact(versionNumber);
        }

        /// <summary>
        /// Returns the tags for a <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The tags for <paramref name="entity"/>.</returns>
        public static ITag[] GetTags<TEntity>(this IServiceProvider serviceProvider, TEntity entity)
        {
            var taggingStrategy = serviceProvider.GetService<ITaggingStrategy<TEntity>>();

            if (taggingStrategy != null)
            {
                return taggingStrategy.GetTags(entity);
            }

            return Array.Empty<ITag>();
        }

        /// <summary>
        /// Determines if the agent is authorized to execute a command on an entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="command">The command.</param>
        /// <param name="claimsPrincipal">The claims of the agent.</param>
        /// <returns><c>true</c> if execution is authorized, or <c>false</c> if execution is not authorized.</returns>
        public static bool IsAuthorized<TEntity>(this IServiceProvider serviceProvider, TEntity entity, ICommand<TEntity> command, ClaimsPrincipal claimsPrincipal)
        {
            var authorizingStrategy = serviceProvider.GetService<IAuthorizingStrategy<TEntity>>();

            if (authorizingStrategy != null)
            {
                return authorizingStrategy.IsAuthorized(entity, command, claimsPrincipal);
            }

            return true;
        }

        /// <summary>
        /// Determines if the next version of an entity should be cached.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="previousEntity">The previous (i.e., already cached) version of the entity, if it exists.</param>
        /// <param name="nextEntity">The next version of the entity.</param>
        /// <returns><c>true</c> if the next version of the entity should be cached, or <c>false</c> if the next version of the entity should not be cached.</returns>
        public static bool ShouldCache<TEntity>(this IServiceProvider serviceProvider, TEntity? previousEntity, TEntity nextEntity)
        {
            var cachingStrategy = serviceProvider.GetService<ICachingStrategy<TEntity>>();

            if (cachingStrategy != null)
            {
                return cachingStrategy.ShouldCache(previousEntity, nextEntity);
            }

            return false;
        }

        /// <summary>
        /// Returns an instance of <typeparamref name="TEntity"/> that represents the current state of an entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="entityId">The id of the entity.</param>
        /// <param name="transactionRepository">The repository which contains relevant entity data.</param>
        /// <param name="snapshotRepository">An optional repository which may or may not contain a snapshot of an entity.</param>
        /// <returns>An instance of <typeparamref name="TEntity"/> that represents the current state of an entity.</returns>
        public static async Task<TEntity> GetEntity<TEntity>(this IServiceProvider serviceProvider, Guid entityId, ITransactionRepository<TEntity> transactionRepository, ISnapshotRepository<TEntity>? snapshotRepository = null)
        {
            TEntity? snapshot = default;

            if (snapshotRepository != null)
            {
                snapshot = await snapshotRepository.GetSnapshot(entityId);
            }

            var entity = snapshot ?? serviceProvider.Construct<TEntity>(entityId);

            var versionNumber = serviceProvider.GetVersionNumber(entity);

            var factQuery = new GetEntityQuery(entityId, versionNumber);

            var facts = await transactionRepository.GetFacts(factQuery);

            entity = entity.Reduce(facts);

            if (serviceProvider.ShouldCache(snapshot, entity) && snapshotRepository != null)
            {
                await snapshotRepository.PutSnapshot(entityId, entity);
            }

            return entity;
        }
    }
}
