using EntityDb.MongoDb.Sessions;
using MongoDB.Driver;
using System;

namespace EntityDb.MongoDb.Transactions
{
    internal sealed class TestModeMongoDbTransactionRepositoryFactory<TEntity> : MongoDbTransactionRepositoryFactory<TEntity>
    {
        public TestModeMongoDbTransactionRepositoryFactory(IServiceProvider serviceProvider, string connectionString, string databaseName) : base(serviceProvider, connectionString, databaseName)
        {
        }

        internal override IMongoDbSession CreateSession(IClientSessionHandle? clientSessionHandle, IMongoDatabase mongoDatabase)
        {
            return new TestModeMongoDbSession(_serviceProvider, clientSessionHandle, mongoDatabase);
        }
    }
}
