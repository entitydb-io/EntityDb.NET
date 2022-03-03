using EntityDb.Abstractions.ValueObjects;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Snapshots;

internal class TestModeSnapshotManager
{
    private readonly List<Id> _snapshotIds = new();

    public void AddSnapshotId(Id snapshotId)
    {
        _snapshotIds.Add(snapshotId);
    }

    public void RemoveSnapshotIds(IEnumerable<Id> snapshotIds)
    {
        _snapshotIds.RemoveAll(snapshotIds.Contains);
    }

    public Id[] GetDeleteSnapshotIds()
    {
        return _snapshotIds.ToArray();
    }
}
