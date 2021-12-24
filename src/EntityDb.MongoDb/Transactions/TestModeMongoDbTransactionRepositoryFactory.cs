using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Sessions;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal class
        TestModeMongoDbTransactionRepositoryFactory<TEntity> : MongoDbTransactionRepositoryFactoryWrapper<TEntity>
    {
        private static readonly TransactionSessionOptions _testTransactionSessionOptions = new()
        {
            ReadOnly = false,
        };

        private (IMongoSession Normal, TestModeMongoSession TestMode)? _sessions;

        public TestModeMongoDbTransactionRepositoryFactory(IMongoDbTransactionRepositoryFactory<TEntity> mongoDbTransactionRepositoryFactory) : base(mongoDbTransactionRepositoryFactory)
        {
        }

        public override async Task<IMongoSession> CreateSession(TransactionSessionOptions transactionSessionOptions)
        {
            if (!_sessions.HasValue)
            {
                var normalSession = await base.CreateSession(_testTransactionSessionOptions);
                var testModeSession = new TestModeMongoSession(normalSession);

                normalSession.StartTransaction();

                _sessions = (normalSession, testModeSession);
            }

            return _sessions.Value.TestMode
                .WithTransactionSessionOptions(transactionSessionOptions);
        }

        public override async ValueTask DisposeAsync()
        {
            if (_sessions.HasValue)
            {
                await _sessions.Value.Normal.AbortTransaction();
                await _sessions.Value.Normal.DisposeAsync();
            }
        }
    }
}
