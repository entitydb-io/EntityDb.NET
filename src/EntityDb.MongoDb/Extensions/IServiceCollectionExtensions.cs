using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.MongoDb.Extensions
{
    /// <summary>
    ///     Extensions for service collections.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "All of the tests in this project are using the auto-provisioning variant.")]
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds a production-ready implementation of <see cref="ITransactionRepositoryFactory{TEntity}" /> to a service
        ///     collection.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity represented in the repository.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="databaseName">The name of the MongoDB database.</param>
        /// <param name="getConnectionString">A function that retrieves the MongoDB connection string.</param>
        /// <param name="testMode">Modifies the behavior of the repository to accomodate tests.</param>
        public static void AddMongoDbTransactions<TEntity>(this IServiceCollection serviceCollection,
            string databaseName, Func<IConfiguration, string> getConnectionString,
            bool testMode = false)
        {
            serviceCollection.AddSingleton<ITransactionRepositoryFactory<TEntity>>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var connectionString = getConnectionString.Invoke(configuration);

                return MongoDbTransactionRepositoryFactory<TEntity>
                    .Create(serviceProvider, connectionString, databaseName)
                    .UseTestMode(testMode);
            });
        }
    }
}
