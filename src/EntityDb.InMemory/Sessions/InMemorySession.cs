using EntityDb.Abstractions.ValueObjects;
using System.Collections.Concurrent;

namespace EntityDb.InMemory.Sessions;

internal class InMemorySession<TSnapshot> : IInMemorySession<TSnapshot>
{
    private readonly ConcurrentDictionary<Pointer, TSnapshot> _dictionary = new();

    public async Task<bool> Insert(Pointer snapshotPointer, TSnapshot snapshot)
    {
        await Task.Yield();

        return _dictionary.TryGetValue(snapshotPointer, out var previousSnapshot)
            ? _dictionary.TryUpdate(snapshotPointer, snapshot, previousSnapshot)
            : _dictionary.TryAdd(snapshotPointer, snapshot);
    }

    public async Task<TSnapshot?> Get(Pointer snapshotPointer)
    {
        await Task.Yield();

        return _dictionary.GetValueOrDefault(snapshotPointer);
    }

    public async Task<bool> Delete(IEnumerable<Pointer> snapshotPointers)
    {
        await Task.Yield();

        return snapshotPointers.All(snapshotPointer => _dictionary.TryRemove(snapshotPointer, out _));
    }
}
