using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Common.Snapshots;

internal class TestModeSnapshotManager<TSnapshot> : DisposableResourceBaseClass
{
    private readonly Dictionary<ISnapshotRepository<TSnapshot>, List<Pointer>> _dictionary = new();

    private List<Pointer> GetStoreedSnapshotPointers(ISnapshotRepository<TSnapshot> snapshotRepository)
    {
        if (!_dictionary.TryGetValue(snapshotRepository, out var storedSnapshotPointers))
        {
            storedSnapshotPointers = new List<Pointer>();

            _dictionary.Add(snapshotRepository, storedSnapshotPointers);
        }

        return storedSnapshotPointers;
    }

    public void AddSnapshotPointer(ISnapshotRepository<TSnapshot> snapshotRepository, Pointer snapshotPointer)
    {
        var storedSnapshotPointers = GetStoreedSnapshotPointers(snapshotRepository);

        storedSnapshotPointers.Add(snapshotPointer);
    }

    public void RemoveSnapshotPointers(ISnapshotRepository<TSnapshot> snapshotRepository,
        IEnumerable<Pointer> snapshotPointers)
    {
        var storedSnapshotPointers = GetStoreedSnapshotPointers(snapshotRepository);

        storedSnapshotPointers.RemoveAll(snapshotPointers.Contains);

        if (storedSnapshotPointers.Count == 0)
        {
            _dictionary.Remove(snapshotRepository);
        }
    }

    /// <remarks>
    ///     This should only be called by the snapshot repository factory.
    /// </remarks>
    public override async ValueTask DisposeAsync()
    {
        foreach (var (snapshotRepository, storedSnapshotPointers) in _dictionary.ToArray())
        {
            await snapshotRepository.DeleteSnapshots(storedSnapshotPointers.ToArray());
        }
    }
}
