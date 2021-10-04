using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Provisioner.Transactions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EntityDb.MongoDb.Provisioner.Extensions
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds an auto-provision, test-mode implementation of <see cref="ITransactionRepositoryFactory{TEntity}" /> to a
        ///     service collection.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity represented in the repository.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="databaseName">The name of the MongoDB database.</param>
        /// <param name="getConnectionString">A function that retrieves the MongoDB connection string.</param>
        /// <remarks>
        ///     The test-mode implementation will not commit transactions. They will be aborted when the
        ///     <see cref="ITransactionRepository{TEntity}" /> is disposed.
        /// </remarks>
        public static void AddAutoProvisionTestModeMongoDbTransactions<TEntity>(
            this IServiceCollection serviceCollection, string databaseName,
            Func<IServiceProvider, string> getConnectionString)
        {
            serviceCollection.AddSingleton<ITransactionRepositoryFactory<TEntity>>(serviceProvider =>
            {
                var connectionString = getConnectionString.Invoke(serviceProvider);

                return AutoProvisionTestModeMongoDbTransactionRepositoryFactory<TEntity>.Create(serviceProvider,
                    connectionString, databaseName);
            });
        }
    }
}
