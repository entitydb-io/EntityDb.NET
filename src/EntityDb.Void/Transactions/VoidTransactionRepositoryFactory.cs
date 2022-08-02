using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Disposables;

namespace EntityDb.Void.Transactions;

internal class VoidTransactionRepositoryFactory : DisposableResourceBaseClass, ITransactionRepositoryFactory
{
    private static readonly Task<ITransactionRepository> VoidTransactionRepositoryTask =
        Task.FromResult(new VoidTransactionRepository() as ITransactionRepository);

    public Task<ITransactionRepository> CreateRepository(
        string transactionSessionOptionsName, CancellationToken cancellationToken = default)
    {
        return VoidTransactionRepositoryTask.WaitAsync(cancellationToken);
    }
}
