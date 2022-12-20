using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

internal abstract class SnapshotTransactionProcessorBase<TSnapshot> : ITransactionProcessor
    where TSnapshot : ISnapshot<TSnapshot>
{
    public abstract Task ProcessTransaction(ITransaction transaction, CancellationToken cancellationToken);

    protected static async Task ProcessTransactionCommands
    (
        ISnapshotRepository<TSnapshot> snapshotRepository,
        SnapshotTransactionCommandProcessorCache<TSnapshot> snapshotTransactionCommandProcessorCache,
        ITransaction transaction,
        ISnapshotTransactionCommandProcessor<TSnapshot> snapshotTransactionCommandProcessor,
        CancellationToken cancellationToken
    )
    {
        var putQueue = new Dictionary<Pointer, TSnapshot>();

        foreach (var transactionCommand in transaction.Commands)
        {
            var snapshots = await snapshotTransactionCommandProcessor.GetSnapshots(transaction, transactionCommand, cancellationToken);

            if (snapshots is not var (previousLatestSnapshot, nextSnapshot))
            {
                continue;
            }

            var snapshotId = nextSnapshot.GetId();

            if (nextSnapshot.ShouldRecordAsLatest(previousLatestSnapshot))
            {
                putQueue[snapshotId] = nextSnapshot;
            }
            else
            {
                snapshotTransactionCommandProcessorCache.PutSnapshot(snapshotId, nextSnapshot);
            }

            if (nextSnapshot.ShouldRecord())
            {
                putQueue[snapshotId + nextSnapshot.GetVersionNumber()] = nextSnapshot;
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        foreach (var (snapshotPointer, snapshot) in putQueue)
        {
            await snapshotRepository.PutSnapshot(snapshotPointer, snapshot, cancellationToken);
        }
    }
}
