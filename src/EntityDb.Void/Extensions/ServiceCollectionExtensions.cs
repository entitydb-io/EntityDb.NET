using EntityDb.Abstractions.Sources;
using EntityDb.Void.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Void.Extensions;

/// <summary>
///     Extensions for service collections.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds a production-ready implementation of <see cref="ISourceRepositoryFactory" /> to a service
    ///     collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <remarks>
    ///     This repository does not do anything.
    /// </remarks>
    public static void AddVoidTransactions(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<ISourceRepositoryFactory, VoidSourceRepositoryFactory>();
    }
}
