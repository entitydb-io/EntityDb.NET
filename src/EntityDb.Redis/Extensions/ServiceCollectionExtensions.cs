using EntityDb.Abstractions.States;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Extensions;
using EntityDb.Common.States;
using EntityDb.Json.Envelopes;
using EntityDb.Redis.ConnectionMultiplexers;
using EntityDb.Redis.States;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Redis.Extensions;

/// <summary>
///     Extensions for service collections.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Don't need coverage for non-test mode.")]
public static class ServiceCollectionExtensions
{
    internal static void AddJsonElementEnvelopeService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IEnvelopeService<byte[]>, JsonBytesEnvelopeService>();
    }

    /// <summary>
    ///     Adds a production-ready implementation of <see cref="IStateRepositoryFactory{TState}" /> to a service
    ///     collection.
    /// </summary>
    /// <typeparam name="TState">The type of the state stored in the repository.</typeparam>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="testMode">Modifies the behavior of the repository to accomodate tests.</param>
    public static void AddRedisStateRepository<TState>(this IServiceCollection serviceCollection, bool testMode = false)
    {
        serviceCollection.AddJsonElementEnvelopeService();

        serviceCollection.AddSingleton<ConnectionMultiplexerFactory>();

        serviceCollection.Add<RedisStateRepositoryFactory<TState>>
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient
        );

        serviceCollection.Add
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient,
            serviceProvider => serviceProvider
                .GetRequiredService<RedisStateRepositoryFactory<TState>>()
                .UseTestMode(testMode)
        );
    }
}
