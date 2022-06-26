using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions;

internal abstract class SnapshotTransactionSubscriberBase<TSnapshot> : TransactionSubscriber
    where TSnapshot : ISnapshot<TSnapshot>
{
    private readonly ISnapshotRepositoryFactory<TSnapshot> _snapshotRepositoryFactory;
    private readonly string _snapshotSessionOptionsName;

    public SnapshotTransactionSubscriberBase
    (
        ISnapshotRepositoryFactory<TSnapshot> snapshotRepositoryFactory,
        string snapshotSessionOptionsName,
        bool testMode
    ) : base(testMode)
    {
        _snapshotRepositoryFactory = snapshotRepositoryFactory;
        _snapshotSessionOptionsName = snapshotSessionOptionsName;
    }

    internal Task<ISnapshotRepository<TSnapshot>> CreateSnapshotRepository()
    {
        return _snapshotRepositoryFactory.CreateRepository(_snapshotSessionOptionsName);
    }

    protected override async Task NotifyAsync(ITransaction transaction)
    {
        await using var snapshotRepository = new SubscriberOptimizedSnapshotRepository<TSnapshot>(await CreateSnapshotRepository());

        foreach (var step in transaction.Steps)
        {
            if (await GetSnapshots(transaction, step, snapshotRepository) is not var (previousMostRecentSnapshot, nextSnapshot))
            {
                continue;
            }

            var snapshotId = nextSnapshot.GetId();

            if (nextSnapshot.ShouldRecordAsMostRecent(previousMostRecentSnapshot))
            {
                await snapshotRepository.PutSnapshot(snapshotId, nextSnapshot);
            }
            else
            {
                snapshotRepository.CacheSnapshot(snapshotId, nextSnapshot);
            }

            if (nextSnapshot.ShouldRecord())
            {
                var nextSnapshotPointer = new Pointer(snapshotId, nextSnapshot.GetVersionNumber());

                await snapshotRepository.PutSnapshot(nextSnapshotPointer, nextSnapshot);
            }
        }

        await snapshotRepository.PutSnapshots();
    }

    protected abstract Task<(TSnapshot? previousMostRecentSnapshot, TSnapshot nextSnapshot)?> GetSnapshots(ITransaction transaction, ITransactionStep transactionStep, ISnapshotRepository<TSnapshot> snapshotRepository);
}
