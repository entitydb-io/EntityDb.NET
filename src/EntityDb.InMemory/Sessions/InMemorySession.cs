using EntityDb.Abstractions.ValueObjects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.InMemory.Sessions;

internal class InMemorySession<TSnapshot> : IInMemorySession<TSnapshot>
{
    private readonly ConcurrentDictionary<Id, TSnapshot> _dictionary = new();
    
    public async Task<bool> Insert(Id snapshotId, TSnapshot snapshot)
    {
        await Task.Yield();
        
        return _dictionary.TryGetValue(snapshotId, out var previousSnapshot)
            ? _dictionary.TryUpdate(snapshotId, snapshot, previousSnapshot)
            : _dictionary.TryAdd(snapshotId, snapshot);
    }

    public async Task<TSnapshot?> Get(Id snapshotId)
    {
        await Task.Yield();

        return _dictionary.GetValueOrDefault(snapshotId);
    }

    public async Task<bool> Delete(Id[] snapshotIds)
    {
        await Task.Yield();

        return snapshotIds.All(snapshotId => _dictionary.TryRemove(snapshotId, out _));
    }
}
