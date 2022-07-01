using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Extensions;
using EntityDb.InMemory.Sessions;
using EntityDb.InMemory.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.InMemory.Extensions;

/// <summary>
///     Extensions for service collections.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Don't need coverage for non-test mode.")]
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds a production-ready implementation of <see cref="ISnapshotRepositoryFactory{TEntity}" /> to a service
    ///     collection.
    /// </summary>
    /// <typeparam name="TSnapshot">The type of the snapshot stored in the repository.</typeparam>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="testMode">Modifies the behavior of the repository to accomodate tests.</param>
    public static void AddInMemorySnapshots<TSnapshot>(this IServiceCollection serviceCollection, bool testMode = false)
    {
        serviceCollection.Add<IInMemorySession<TSnapshot>>
        (
            testMode ? ServiceLifetime.Singleton : ServiceLifetime.Transient,
            _ => new InMemorySession<TSnapshot>()
        );

        serviceCollection.AddScoped(serviceProvider => ActivatorUtilities
            .CreateInstance<InMemorySnapshotRepositoryFactory<TSnapshot>>(serviceProvider)
            .UseTestMode(testMode));
    }
}
