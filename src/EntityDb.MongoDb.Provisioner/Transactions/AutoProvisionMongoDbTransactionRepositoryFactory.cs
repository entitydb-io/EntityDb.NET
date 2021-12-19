using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Provisioner.Extensions;
using EntityDb.MongoDb.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Transactions
{
    internal sealed class
        AutoProvisionMongoDbTransactionRepositoryFactory<TEntity> : MongoDbTransactionRepositoryFactory<
            TEntity>
    {
        private bool _needToProvision = true;

        public AutoProvisionMongoDbTransactionRepositoryFactory(IOptionsFactory<TransactionSessionOptions> optionsFactory, ILoggerFactory loggerFactory,
            IResolvingStrategyChain resolvingStrategyChain, string connectionString, string databaseName) : base(optionsFactory,
            loggerFactory, resolvingStrategyChain, connectionString, databaseName)
        {
        }

        protected override async Task<IMongoClient> CreateClient(TransactionSessionOptions transactionSessionOptions)
        {
            var mongoClient = await base.CreateClient(transactionSessionOptions);

            if (!_needToProvision)
            {
                return mongoClient;
            }

            _needToProvision = false;

            await mongoClient.ProvisionCollections(_databaseName);

            return mongoClient;
        }

        public static new AutoProvisionMongoDbTransactionRepositoryFactory<TEntity> Create(
            IServiceProvider serviceProvider, string connectionString, string databaseName)
        {
            return ActivatorUtilities.CreateInstance<AutoProvisionMongoDbTransactionRepositoryFactory<TEntity>>
            (
                serviceProvider,
                connectionString,
                databaseName
            );
        }
    }
}
