using EntityDb.Abstractions.Snapshots;
using EntityDb.Redis.Snapshots;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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
        public static void AddRedisSnapshots<TEntity>(this IServiceCollection serviceCollection, string keyNamespace,
            Func<IConfiguration, string> getConnectionString)
        {
            serviceCollection.AddSingleton<ISnapshotRepositoryFactory<TEntity>>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var connectionString = getConnectionString.Invoke(configuration);

                return RedisSnapshotRepositoryFactory<TEntity>.Create(serviceProvider, connectionString, keyNamespace);
            });
        }
    }
}
