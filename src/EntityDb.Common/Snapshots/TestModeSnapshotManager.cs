using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Snapshots;

internal class TestModeSnapshotManager
{
    private readonly List<Guid> _snapshotIds = new();

    public void AddSnapshotId(Guid snapshotId)
    {
        _snapshotIds.Add(snapshotId);
    }

    public void RemoveSnapshotIds(IEnumerable<Guid> snapshotIds)
    {
        _snapshotIds.RemoveAll(snapshotIds.Contains);
    }

    public Guid[] GetDeleteSnapshotIds()
    {
        return _snapshotIds.ToArray();
    }
}
