using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Sessions;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal class
        TestModeMongoDbTransactionRepositoryFactory<TEntity> : MongoDbTransactionRepositoryFactoryWrapper<TEntity>
    {
        private static readonly TransactionSessionOptions _testTransactionSessionOptions = new()
        {
            WriteTimeout = TimeSpan.FromMinutes(1),
        };

        private WriteMongoSession? _writeSession;

        public TestModeMongoDbTransactionRepositoryFactory(IMongoDbTransactionRepositoryFactory<TEntity> mongoDbTransactionRepositoryFactory) : base(mongoDbTransactionRepositoryFactory)
        {
        }

        public override async Task<IMongoSession> CreateSession(TransactionSessionOptions transactionSessionOptions)
        {
            if (_writeSession == null)
            {
                _writeSession = await _mongoDbTransactionRepositoryFactory.CreateWriteSession(_testTransactionSessionOptions);

                _writeSession.StartTransaction();
            }

            if (transactionSessionOptions.ReadOnly)
            {
                return new TestModeMongoSessionWrapper
                (
                    MongoSession: new ReadOnlyMongoSession
                    (
                        _writeSession.MongoDatabase,
                        _writeSession.LoggerFactory,
                        _writeSession.ResolvingStrategyChain,
                        transactionSessionOptions
                    ),
                    ReadOnly: true
                );
            }

            return new TestModeMongoSessionWrapper
            (
                MongoSession: new WriteMongoSession
                (
                    _writeSession.MongoDatabase,
                    _writeSession.ClientSessionHandle,
                    _writeSession.LoggerFactory,
                    _writeSession.ResolvingStrategyChain,
                    transactionSessionOptions
                ),
                ReadOnly: false
            );
        }

        public override async ValueTask DisposeAsync()
        {
            if (_writeSession != null)
            {
                await _writeSession.AbortTransaction();

                await _writeSession.DisposeAsync();
            }
        }
    }
}
