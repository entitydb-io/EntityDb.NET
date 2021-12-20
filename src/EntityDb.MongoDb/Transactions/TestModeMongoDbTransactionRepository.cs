using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal class TestModeMongoDbTransactionRepository<TEntity> : TransactionRepositoryWrapper<TEntity>
    {
        private readonly TestModeTransactionManager _testModeTransactionManager;
        private readonly TransactionTestMode _transactionTestMode;

        public TestModeMongoDbTransactionRepository(ITransactionRepository<TEntity> transactionRepository, TestModeTransactionManager testModeTransactionManager, TransactionTestMode transactionTestMode) : base(transactionRepository)
        {
            _testModeTransactionManager = testModeTransactionManager;
            _transactionTestMode = transactionTestMode;

            if (transactionTestMode == TransactionTestMode.AllRepositoriesDisposed)
            {
                _testModeTransactionManager.Hold(nameof(TestModeMongoDbTransactionRepository<TEntity>));
            }
        }

        public override ValueTask DisposeAsync()
        {
            if (_transactionTestMode == TransactionTestMode.AllRepositoriesDisposed)
            {
                _testModeTransactionManager.Release(nameof(TestModeMongoDbTransactionRepository<TEntity>));
            }

            return base.DisposeAsync();
        }
    }
}
