using EntityDb.Abstractions.Transactions;
using System.Threading.Tasks;

namespace EntityDb.Void.Transactions
{
    internal class VoidTransactionRepositoryFactory<TEntity> : ITransactionRepositoryFactory<TEntity>
    {
        private static readonly Task<ITransactionRepository<TEntity>> _voidTransactionRepositoryTask =
            Task.FromResult(new VoidTransactionRepository<TEntity>() as ITransactionRepository<TEntity>);

        public Task<ITransactionRepository<TEntity>> CreateRepository(
            ITransactionSessionOptions transactionSessionOptions)
        {
            return _voidTransactionRepositoryTask;
        }
    }
}
