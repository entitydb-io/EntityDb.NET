using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Transactions;
using EntityDb.MongoDb.Provisioner.Extensions;
using EntityDb.MongoDb.Transactions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Transactions
{
    internal sealed class AutoProvisionTestModeMongoDbTransactionRepositoryFactory<TEntity> : TestModeMongoDbTransactionRepositoryFactory<TEntity>
    {
        private bool _needToProvision = true;
        
        public AutoProvisionTestModeMongoDbTransactionRepositoryFactory(ILoggerFactory loggerFactory, IResolvingStrategyChain resolvingStrategyChain, string connectionString, string databaseName) : base(loggerFactory, resolvingStrategyChain, connectionString, databaseName)
        {
        }

        protected override async Task<IMongoClient> CreateClient(ITransactionSessionOptions transactionSessionOptions)
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

        public static new AutoProvisionTestModeMongoDbTransactionRepositoryFactory<TEntity> Create(IServiceProvider serviceProvider, string connectionString, string databaseName)
        {
            return ActivatorUtilities.CreateInstance<AutoProvisionTestModeMongoDbTransactionRepositoryFactory<TEntity>>
            (
                serviceProvider,
                connectionString,
                databaseName
            );
        }
    }
}
