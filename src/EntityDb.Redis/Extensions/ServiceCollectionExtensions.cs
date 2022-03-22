using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Extensions;
using EntityDb.Redis.Envelopes;
using EntityDb.Redis.Snapshots;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace EntityDb.Redis.Extensions;

/// <summary>
///     Extensions for service collections.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Don't need coverage for non-test mode.")]
public static class ServiceCollectionExtensions
{
    internal static void AddJsonElementEnvelopeService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IEnvelopeService<JsonElement>, JsonElementEnvelopeService>();
    }
    
    /// <summary>
    ///     Adds a production-ready implementation of <see cref="ISnapshotRepositoryFactory{TEntity}" /> to a service
    ///     collection.
    /// </summary>
    /// <typeparam name="TSnapshot">The type of the snapshot stored in the repository.</typeparam>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="keyNamespace">The namespace used to build a Redis key.</param>
    /// <param name="getConnectionString">A function that retrieves the Redis connection string.</param>
    /// <param name="testMode">Modifies the behavior of the repository to accomodate tests.</param>
    public static void AddRedisSnapshots<TSnapshot>(this IServiceCollection serviceCollection, string keyNamespace,
        Func<IConfiguration, string> getConnectionString, bool testMode = false)
    {
        serviceCollection.AddJsonElementEnvelopeService();
        
        serviceCollection.Add
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Scoped,
            serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var connectionString = getConnectionString.Invoke(configuration);

                return RedisSnapshotRepositoryFactory<TSnapshot>
                    .Create(serviceProvider, connectionString, keyNamespace)
                    .UseTestMode(testMode);
            }
        );
    }
}
