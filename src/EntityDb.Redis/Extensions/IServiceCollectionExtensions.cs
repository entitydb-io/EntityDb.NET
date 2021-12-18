using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using EntityDb.Redis.Snapshots;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="keyNamespace">The namespace used to build a Redis key.</param>
        /// <param name="getConnectionString">A function that retrieves the Redis connection string.</param>
        /// <remarks>
        ///     The production-ready implementation will store snapshots as they come in. If you need write an integration test,
        ///     consider using
        ///     <see cref="AddTestModeRedisSnapshots{TEntity}(IServiceCollection, string, Func{IConfiguration, string})" />
        ///     instead.
        /// </remarks>
        [ExcludeFromCodeCoverage(Justification = "Tests use TestMode.")]
        public static void AddRedisSnapshots<TEntity>(this IServiceCollection serviceCollection, string keyNamespace,
            Func<IConfiguration, string> getConnectionString)
        {
            serviceCollection.AddSingleton<ITransactionSubscriber<TEntity>>(serviceProvider =>
                SnapshottingTransactionSubscriber<TEntity>.Create(serviceProvider, false));
            
            serviceCollection.AddSingleton<ISnapshotRepositoryFactory<TEntity>>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var connectionString = getConnectionString.Invoke(configuration);

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
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="keyNamespace">The namespace used to build a Redis key.</param>
        /// <param name="getConnectionString">A function that retrieves the Redis connection string.</param>
        /// <remarks>
        ///     The test-mode implementation will store snapshots as they come in, but the snapshots will be automatically removed
        ///     when the repository is disposed.
        /// </remarks>
        public static void AddTestModeRedisSnapshots<TEntity>(this IServiceCollection serviceCollection,
            string keyNamespace, Func<IConfiguration, string> getConnectionString)
        {
            serviceCollection.AddSingleton<ITransactionSubscriber<TEntity>>(serviceProvider =>
                SnapshottingTransactionSubscriber<TEntity>.Create(serviceProvider, true));
            
            serviceCollection.AddSingleton<ISnapshotRepositoryFactory<TEntity>>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var connectionString = getConnectionString.Invoke(configuration);

                return TestModeRedisSnapshotRepositoryFactory<TEntity>.Create(serviceProvider, connectionString,
                    keyNamespace);
            });
        }
    }
}
