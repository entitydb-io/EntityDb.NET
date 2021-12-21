using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Snapshots
{
    internal class TestModeSnapshotManager
    {
        private readonly List<Guid> _entityIds = new();

        public void AddEntityId(Guid entityId)
        {
            _entityIds.Add(entityId);
        }

        public void RemoveEntityIds(Guid[] entityIds)
        {
            _entityIds.RemoveAll(entityIds.Contains);
        }

        public Guid[] GetDeleteEntityIds()
        {
            return _entityIds.ToArray();
        }
    }
}
