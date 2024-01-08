using EntityDb.Abstractions.Sources;
using EntityDb.Common.Disposables;

namespace EntityDb.Void.Transactions;

internal class VoidSourceRepositoryFactory : DisposableResourceBaseClass, ISourceRepositoryFactory
{
    private static readonly Task<ISourceRepository> VoidTransactionRepositoryTask =
        Task.FromResult(new VoidSourceRepository() as ISourceRepository);

    public Task<ISourceRepository> CreateRepository(
        string sourceSessionOptionsName, CancellationToken cancellationToken = default)
    {
        return VoidTransactionRepositoryTask.WaitAsync(cancellationToken);
    }
}
