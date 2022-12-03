using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;

namespace EntityDb.Common.Snapshots;

internal class TestModeSnapshotManager<TSnapshot> : DisposableResourceBaseClass
{
    private readonly Dictionary<ISnapshotRepository<TSnapshot>, List<Pointer>> _dictionary = new();

    private List<Pointer> GetStoredSnapshotPointers(ISnapshotRepository<TSnapshot> snapshotRepository)
    {
        if (_dictionary.TryGetValue(snapshotRepository, out var storedSnapshotPointers))
        {
            return storedSnapshotPointers;
        }

        storedSnapshotPointers = new List<Pointer>();

        _dictionary.Add(snapshotRepository, storedSnapshotPointers);

        return storedSnapshotPointers;
    }

    public void AddSnapshotPointer(ISnapshotRepository<TSnapshot> snapshotRepository, Pointer snapshotPointer)
    {
        var storedSnapshotPointers = GetStoredSnapshotPointers(snapshotRepository);

        storedSnapshotPointers.Add(snapshotPointer);
    }

    public void RemoveSnapshotPointers(ISnapshotRepository<TSnapshot> snapshotRepository,
        IEnumerable<Pointer> snapshotPointers)
    {
        var storedSnapshotPointers = GetStoredSnapshotPointers(snapshotRepository);

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
