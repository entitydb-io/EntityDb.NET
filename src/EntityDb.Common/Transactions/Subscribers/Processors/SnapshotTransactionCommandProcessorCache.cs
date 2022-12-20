using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

internal class SnapshotTransactionCommandProcessorCache<TSnapshot>
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
