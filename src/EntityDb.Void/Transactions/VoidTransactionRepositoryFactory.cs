using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Disposables;
using System.Threading.Tasks;

namespace EntityDb.Void.Transactions;

internal class VoidTransactionRepositoryFactory<TEntity> : DisposableResourceBaseClass, ITransactionRepositoryFactory<TEntity>
{
    private static readonly Task<ITransactionRepository<TEntity>> VoidTransactionRepositoryTask =
        Task.FromResult(new VoidTransactionRepository<TEntity>() as ITransactionRepository<TEntity>);

    public Task<ITransactionRepository<TEntity>> CreateRepository(
        string transactionSessionOptionsName)
    {
        return VoidTransactionRepositoryTask;
    }
}
