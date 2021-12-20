using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Sessions;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal class TestModeMongoDbTransactionRepositoryFactory<TEntity> : MongoDbTransactionRepositoryFactoryWrapper<TEntity>
    {
        private readonly TestModeTransactionManager _testModeTransactionManager = new();
        private readonly TransactionTestMode _transactionTestMode;

        private IMongoClient? _primaryClientSingleton;
        private IMongoClient? _secondaryClientSingleton;
        private IClientSessionHandle? _clientSessionHandleSingleton;

        public TestModeMongoDbTransactionRepositoryFactory(IMongoDbTransactionRepositoryFactory<TEntity> mongoDbTransactionRepositoryFactory, TransactionTestMode transactionTestMode) : base(mongoDbTransactionRepositoryFactory)
        {
            _transactionTestMode = transactionTestMode;
        }

        public override IMongoClient CreatePrimaryClient()
        {
            if (_transactionTestMode == TransactionTestMode.RepositoryFactoryDisposed)
            {
                return _primaryClientSingleton ??= base.CreatePrimaryClient();
            }

            return base.CreatePrimaryClient();
        }

        public override IMongoClient CreateSecondaryClient()
        {
            if (_transactionTestMode == TransactionTestMode.RepositoryFactoryDisposed)
            {
                return _secondaryClientSingleton ??= base.CreateSecondaryClient();
            }

            return base.CreateSecondaryClient();
        }

        public override async Task<IClientSessionHandle> CreateClientSessionHandle()
        {
            if (_transactionTestMode == TransactionTestMode.RepositoryFactoryDisposed)
            {
                return _clientSessionHandleSingleton ??= await base.CreateClientSessionHandle();
            }

            return await base.CreateClientSessionHandle();
        }

        public override async Task<MongoDbTransactionObjects> CreateObjects(TransactionSessionOptions transactionSessionOptions)
        {
            var (mongoSession, mongoClient) = await base.CreateObjects(transactionSessionOptions);

            if (mongoSession is MongoSession concreteMongoSession)
            {
                mongoSession = new TestModeMongoSession(concreteMongoSession.ClientSessionHandle, _testModeTransactionManager);
            }

            return new(mongoSession, mongoClient);
        }

        public override ITransactionRepository<TEntity> CreateRepository(TransactionSessionOptions transactionSessionOptions, IMongoSession? mongoSession, IMongoClient mongoClient)
        {
            return base
                .CreateRepository(transactionSessionOptions, mongoSession, mongoClient)
                .UseTestMode(_testModeTransactionManager, _transactionTestMode);
        }

        public override async ValueTask DisposeAsync()
        {
            if (_transactionTestMode == TransactionTestMode.AllRepositoriesDisposed && _clientSessionHandleSingleton != null)
            {
                await _clientSessionHandleSingleton.AbortTransactionAsync();
            }

            await base.DisposeAsync();
        }
    }
}
