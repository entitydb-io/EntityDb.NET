using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Entities;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.Common.Extensions
{
    /// <summary>
    ///     Extensions for service providers.
    /// </summary>
    public static class IServiceProviderExtensions
    {
        /// <summary>
        ///     Returns the <see cref="IAgent" /> associated with the current service scope.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        public static IAgent GetAgent(this IServiceProvider serviceProvider)
        {
            IAgentAccessor? agentAccessor = serviceProvider.GetRequiredService<IAgentAccessor>();

            return agentAccessor.GetAgent();
        }

        /// <summary>
        ///     Returns a new instance of <see cref="TransactionBuilder{TEntity}" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity for which a transaction is to be built.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        public static TransactionBuilder<TEntity> GetTransactionBuilder<TEntity>(this IServiceProvider serviceProvider)
        {
            return new TransactionBuilder<TEntity>(serviceProvider);
        }

        /// <summary>
        ///     Returns the resolved <see cref="Type" /> or throws if the <see cref="Type" /> cannot be resolved.
        /// </summary>
        /// <param name="serviceProvider">The servie provider.</param>
        /// <param name="headers">Describes the type that needs to be resolved.</param>
        /// <returns>The resolved <see cref="Type" />.</returns>
        public static Type ResolveType(this IServiceProvider serviceProvider,
            IReadOnlyDictionary<string, string> headers)
        {
            IResolvingStrategyChain? resolvingStrategyChain =
                serviceProvider.GetRequiredService<IResolvingStrategyChain>();

            return resolvingStrategyChain.ResolveType(headers);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ITransactionRepository{TEntity}" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="transactionSessionOptions">The agent's use case for the repository.</param>
        /// <returns>A new instance of <see cref="ITransactionRepository{TEntity}" />.</returns>
        public static Task<ITransactionRepository<TEntity>> CreateTransactionRepository<TEntity>(
            this IServiceProvider serviceProvider, ITransactionSessionOptions transactionSessionOptions)
        {
            ITransactionRepositoryFactory<TEntity>? transactionRepositoryFactory =
                serviceProvider.GetRequiredService<ITransactionRepositoryFactory<TEntity>>();

            return transactionRepositoryFactory.CreateRepository(transactionSessionOptions);
        }

        /// <summary>
        ///     Create a new instance of <see cref="ISnapshotRepository{TEntity}" />
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="snapshotSessionOptions">The agent's use case for the repository.</param>
        /// <returns>A new instance of <see cref="ISnapshotRepository{TEntity}" />.</returns>
        public static Task<ISnapshotRepository<TEntity>> CreateSnapshotRepository<TEntity>(
            this IServiceProvider serviceProvider, ISnapshotSessionOptions snapshotSessionOptions)
        {
            ISnapshotRepositoryFactory<TEntity>? snapshotRepositoryFactory =
                serviceProvider.GetRequiredService<ISnapshotRepositoryFactory<TEntity>>();

            return snapshotRepositoryFactory.CreateRepository(snapshotSessionOptions);
        }

        /// <summary>
        ///     Create a new instance of <see cref="IEntityRepository{TEntity}" />
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="transactionSessionOptions">The agent's use case for the nested transaction repository.</param>
        /// <param name="snapshotSessionOptions">The agent's use case for the nested snapshot repository, if needed.</param>
        /// <returns></returns>
        public static async Task<IEntityRepository<TEntity>> CreateEntityRepository<TEntity>(
            this IServiceProvider serviceProvider, ITransactionSessionOptions transactionSessionOptions,
            ISnapshotSessionOptions? snapshotSessionOptions = null)
        {
            ITransactionRepositoryFactory<TEntity>? transactionRepositoryFactory =
                serviceProvider.GetRequiredService<ITransactionRepositoryFactory<TEntity>>();

            ITransactionRepository<TEntity>? transactionRepository =
                await transactionRepositoryFactory.CreateRepository(transactionSessionOptions);

            if (snapshotSessionOptions == null)
            {
                return new EntityRepository<TEntity>(serviceProvider, transactionRepository);
            }

            ISnapshotRepositoryFactory<TEntity>? snapshotRepositoryFactory =
                serviceProvider.GetRequiredService<ISnapshotRepositoryFactory<TEntity>>();

            ISnapshotRepository<TEntity>? snapshotRepository =
                await snapshotRepositoryFactory.CreateRepository(snapshotSessionOptions);

            return new EntityRepository<TEntity>(serviceProvider, transactionRepository, snapshotRepository);
        }

        /// <summary>
        ///     Returns a new instance of a <typeparamref name="TEntity" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="entityId">The id of the entity.</param>
        /// <returns>A new instance of <typeparamref name="TEntity" />.</returns>
        public static TEntity Construct<TEntity>(this IServiceProvider serviceProvider, Guid entityId)
        {
            IConstructingStrategy<TEntity>? constructingStrategy =
                serviceProvider.GetRequiredService<IConstructingStrategy<TEntity>>();

            return constructingStrategy.Construct(entityId);
        }

        /// <summary>
        ///     Returns the version number of a <typeparamref name="TEntity" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The version number of <paramref name="entity" />.</returns>
        public static ulong GetVersionNumber<TEntity>(this IServiceProvider serviceProvider, TEntity entity)
        {
            IVersioningStrategy<TEntity>? versioningStrategy =
                serviceProvider.GetRequiredService<IVersioningStrategy<TEntity>>();

            return versioningStrategy.GetVersionNumber(entity);
        }

        /// <summary>
        ///     Creates a new version number modifier for an entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="versionNumber">The desired version number.</param>
        /// <returns>A new version number modifier for on an entity.</returns>
        public static IFact<TEntity> GetVersionNumberFact<TEntity>(this IServiceProvider serviceProvider,
            ulong versionNumber)
        {
            IVersioningStrategy<TEntity>? versioningStrategy =
                serviceProvider.GetRequiredService<IVersioningStrategy<TEntity>>();

            return versioningStrategy.GetVersionNumberFact(versionNumber);
        }

        /// <summary>
        ///     Returns the leases for a <typeparamref name="TEntity" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The leases for <paramref name="entity" />.</returns>
        public static ILease[] GetLeases<TEntity>(this IServiceProvider serviceProvider, TEntity entity)
        {
            ILeasingStrategy<TEntity>? leasingStrategy = serviceProvider.GetService<ILeasingStrategy<TEntity>>();

            if (leasingStrategy != null)
            {
                return leasingStrategy.GetLeases(entity);
            }

            return Array.Empty<ILease>();
        }

        /// <summary>
        ///     Returns the tags for a <typeparamref name="TEntity" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The tags for <paramref name="entity" />.</returns>
        public static ITag[] GetTags<TEntity>(this IServiceProvider serviceProvider, TEntity entity)
        {
            ITaggingStrategy<TEntity>? taggingStrategy = serviceProvider.GetService<ITaggingStrategy<TEntity>>();

            if (taggingStrategy != null)
            {
                return taggingStrategy.GetTags(entity);
            }

            return Array.Empty<ITag>();
        }

        /// <summary>
        ///     Determines if the agent is authorized to execute a command on an entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="command">The command.</param>
        /// <returns><c>true</c> if execution is authorized, or <c>false</c> if execution is not authorized.</returns>
        public static bool IsAuthorized<TEntity>(this IServiceProvider serviceProvider, TEntity entity,
            ICommand<TEntity> command)
        {
            IAuthorizingStrategy<TEntity>? authorizingStrategy =
                serviceProvider.GetService<IAuthorizingStrategy<TEntity>>();

            if (authorizingStrategy != null)
            {
                IAgent? agent = serviceProvider.GetAgent();

                return authorizingStrategy.IsAuthorized(entity, command, agent);
            }

            return true;
        }

        /// <summary>
        ///     Determines if the next version of an entity should be put into snapshot storage.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="previousEntity">The previous version of the entity, if it exists.</param>
        /// <param name="nextEntity">The next version of the entity.</param>
        /// <returns>
        ///     <c>true</c> if the next version of the entity should be cached, or <c>false</c> if the next version of the
        ///     entity should not be put into snapshot storage.
        /// </returns>
        public static bool ShouldPutSnapshot<TEntity>(this IServiceProvider serviceProvider, TEntity? previousEntity,
            TEntity nextEntity)
        {
            ISnapshottingStrategy<TEntity>? snapshottingStrategy =
                serviceProvider.GetService<ISnapshottingStrategy<TEntity>>();

            if (snapshottingStrategy != null)
            {
                return snapshottingStrategy.ShouldPutSnapshot(previousEntity, nextEntity);
            }

            return false;
        }
    }
}
