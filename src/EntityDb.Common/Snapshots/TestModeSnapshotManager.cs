using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal class TestModeSnapshotManager<TSnapshot> : DisposableResourceBaseClass
{
    private readonly Dictionary<ISnapshotRepository<TSnapshot>, List<Id>> _dictionary = new();

    private List<Id> GetStoreSnapshotIds(ISnapshotRepository<TSnapshot> snapshotRepository)
    {
        if (!_dictionary.TryGetValue(snapshotRepository, out var storedSnapshotIds))
        {
            _dictionary.Add(snapshotRepository, storedSnapshotIds = new List<Id>());
        }

        return storedSnapshotIds;
    }
    
    public void AddSnapshotId(ISnapshotRepository<TSnapshot> snapshotRepository, Id snapshotId)
    {
        var storedSnapshotIds = GetStoreSnapshotIds(snapshotRepository);
        
        storedSnapshotIds.Add(snapshotId);
    }

    public void RemoveSnapshotIds(ISnapshotRepository<TSnapshot> snapshotRepository, IEnumerable<Id> snapshotIds)
    {
        var storedSnapshotIds = GetStoreSnapshotIds(snapshotRepository);
        
        storedSnapshotIds.RemoveAll(snapshotIds.Contains);

        if (storedSnapshotIds.Count == 0)
        {
            _dictionary.Remove(snapshotRepository);
        }
    }

    /// <remarks>
    ///     This should only be called by the snapshot repository factory.
    /// </remarks>
    public override async ValueTask DisposeAsync()
    {
        foreach (var (snapshotRepository, storedSnapshotIds) in _dictionary.ToArray())
        {
            await snapshotRepository.DeleteSnapshots(storedSnapshotIds.ToArray());
        }
    }
}
