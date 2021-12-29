using EntityDb.Abstractions.Transactions;
using EntityDb.Void.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Void.Extensions
{
    /// <summary>
    ///     Extensions for service collections.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds a production-ready implementation of <see cref="ITransactionRepositoryFactory{TEntity}" /> to a service
        ///     collection.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity represented in the repository.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <remarks>
        ///     This repository does not do anything.
        /// </remarks>
        public static void AddVoidTransactions<TEntity>(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<ITransactionRepositoryFactory<TEntity>, VoidTransactionRepositoryFactory<TEntity>>();
        }
    }
}
