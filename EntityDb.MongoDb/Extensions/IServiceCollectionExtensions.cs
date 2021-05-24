using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Transactions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.MongoDb.Extensions
{
    /// <summary>
    /// Extensions for service collections.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a production-ready implementation of <see cref="ITransactionRepositoryFactory{TEntity}"/> to a service collection.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity represented in the repository.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="databaseName">The name of the MongoDB database.</param>
        /// <param name="getConnectionString">A function that retrieves the MongoDB connection string.</param>
        /// <remarks>
        /// The production-ready implementation will commit transactions as they are passed in. If you need write an integration test, consider using <see cref="AddTestModeMongoDbTransactions{TEntity}(IServiceCollection, string, Func{IServiceProvider, string})"/> instead.
        /// </remarks>
        [ExcludeFromCodeCoverage(Justification = "Tests use TestMode.")]
        public static void AddMongoDbTransactions<TEntity>(this IServiceCollection serviceCollection, string databaseName, Func<IServiceProvider, string> getConnectionString)
        {
            serviceCollection.AddScoped<ITransactionRepositoryFactory<TEntity>>((serviceProvider) =>
            {
                var connectionString = getConnectionString.Invoke(serviceProvider);

                return ActivatorUtilities.CreateInstance<MongoDbTransactionRepositoryFactory<TEntity>>(serviceProvider, connectionString, databaseName);
            });
        }

        /// <summary>
        /// Adds an test-mode implementation of <see cref="ITransactionRepositoryFactory{TEntity}"/> to a service collection.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity represented in the repository.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="databaseName">The name of the MongoDB database.</param>
        /// <param name="getConnectionString">A function that retrieves the MongoDB connection string.</param>
        /// <remarks>
        /// The test-mode implementation will not commit transactions. They will be aborted when the <see cref="ITransactionRepository{TEntity}"/> is disposed.
        /// </remarks>
        public static void AddTestModeMongoDbTransactions<TEntity>(this IServiceCollection serviceCollection, string databaseName, Func<IServiceProvider, string> getConnectionString)
        {
            serviceCollection.AddScoped<ITransactionRepositoryFactory<TEntity>>((serviceProvider) =>
            {
                var connectionString = getConnectionString.Invoke(serviceProvider);

                return ActivatorUtilities.CreateInstance<TestModeMongoDbTransactionRepositoryFactory<TEntity>>(serviceProvider, connectionString, databaseName);
            });
        }
    }
}
