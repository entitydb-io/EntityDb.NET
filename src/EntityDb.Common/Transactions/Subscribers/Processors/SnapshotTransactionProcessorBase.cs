using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

internal abstract class SnapshotTransactionProcessorBase<TSnapshot> : ITransactionProcessor
    where TSnapshot : ISnapshot<TSnapshot>
{
    public abstract Task ProcessTransaction(ITransaction transaction, CancellationToken cancellationToken);

    protected static SnapshotCache CreateSnapshotCache()
    {
        return new SnapshotCache();
    }

    protected static async Task ProcessTransactionSteps(ISnapshotRepository<TSnapshot> snapshotRepository,
        SnapshotCache snapshotCache,
        ITransaction transaction, Func<IAppendCommandTransactionStep, Task<(TSnapshot?, TSnapshot)?>> getSnapshots,
        CancellationToken cancellationToken)
    {
        var putQueue = new Dictionary<Pointer, TSnapshot>();

        foreach (var transactionStep in transaction.Steps)
        {
            if (transactionStep is not IAppendCommandTransactionStep appendCommandTransactionStep)
            {
                continue;
            }

            if (await getSnapshots.Invoke(appendCommandTransactionStep) is not var (previousLatestSnapshot, nextSnapshot
                ))
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
                snapshotCache.PutSnapshot(snapshotId, nextSnapshot);
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

    protected class SnapshotCache
    {
        private readonly Dictionary<Pointer, TSnapshot> _cache = new();

        public void PutSnapshot(Pointer snapshotPointer, TSnapshot snapshot)
        {
            _cache[snapshotPointer] = snapshot;
        }

        public TSnapshot? GetSnapshotOrDefault(Pointer snapshotPointer)
        {
            return _cache.GetValueOrDefault(snapshotPointer);
        }
    }
}
