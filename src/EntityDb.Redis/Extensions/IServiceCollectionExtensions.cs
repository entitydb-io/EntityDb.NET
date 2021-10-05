using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Entities;
using EntityDb.Common.Transactions;
using EntityDb.Redis.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Redis.Extensions
{
    /// <summary>
    ///     Extensions for service collections.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds a production-ready implementation of <see cref="ISnapshotRepositoryFactory{TEntity}" /> to a service
        ///     collection.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity represented in the repository.</typeparam>
        /// <typeparam name="TSnapshottingStrategy">The type of the strategy used to decide if a snapshot should be recorded.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="keyNamespace">The namespace used to build a Redis key.</param>
        /// <param name="getConnectionString">A function that retrieves the Redis connection string.</param>
        /// <remarks>
        ///     The production-ready implementation will store snapshots as they come in. If you need write an integration test,
        ///     consider using
        ///     <see cref="AddTestModeRedisSnapshots{TEntity,TSnapshottingStrategy}(IServiceCollection, string, Func{IServiceProvider, string})" />
        ///     instead.
        /// </remarks>
        [ExcludeFromCodeCoverage(Justification = "Tests use TestMode.")]
        public static void AddRedisSnapshots<TEntity, TSnapshottingStrategy>(this IServiceCollection serviceCollection, string keyNamespace,
            Func<IServiceProvider, string> getConnectionString)
            where TSnapshottingStrategy : class, ISnapshottingStrategy<TEntity>
        {
            serviceCollection.AddSingleton<ITransactionSubscriber<TEntity>, SnapshottingTransactionSubscriber<TEntity>>();
            
            serviceCollection.AddSingleton<ISnapshottingStrategy<TEntity>, TSnapshottingStrategy>();
            
            serviceCollection.AddSingleton<ISnapshotRepositoryFactory<TEntity>>(serviceProvider =>
            {
                var connectionString = getConnectionString.Invoke(serviceProvider);

                return RedisSnapshotRepositoryFactory<TEntity>.Create(serviceProvider, connectionString, keyNamespace);
            });
        }

        /// <summary>
        ///     Any keys which are added during the lifetime of the repository will be removed when the repository is disposed.
        /// </summary>
        /// <summary>
        ///     Adds a test-mode implementation of <see cref="ISnapshotRepositoryFactory{TEntity}" /> to a service collection.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity represented in the repository.</typeparam>
        /// <typeparam name="TSnapshottingStrategy">The type of the strategy used to decide if a snapshot should be recorded.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="keyNamespace">The namespace used to build a Redis key.</param>
        /// <param name="getConnectionString">A function that retrieves the Redis connection string.</param>
        /// <remarks>
        ///     The test-mode implementation will store snapshots as they come in, but the snapshots will be automatically removed
        ///     when the repository is disposed.
        /// </remarks>
        public static void AddTestModeRedisSnapshots<TEntity, TSnapshottingStrategy>(this IServiceCollection serviceCollection,
            string keyNamespace, Func<IServiceProvider, string> getConnectionString)
            where TSnapshottingStrategy : class, ISnapshottingStrategy<TEntity>
        {
            serviceCollection.AddSingleton<ITransactionSubscriber<TEntity>, SnapshottingTransactionSubscriber<TEntity>>();
            
            serviceCollection.AddSingleton<ISnapshottingStrategy<TEntity>, TSnapshottingStrategy>();

            serviceCollection.AddSingleton<ISnapshotRepositoryFactory<TEntity>>(serviceProvider =>
            {
                var connectionString = getConnectionString.Invoke(serviceProvider);

                return TestModeRedisSnapshotRepositoryFactory<TEntity>.Create(serviceProvider, connectionString,
                    keyNamespace);
            });
        }
    }
}
