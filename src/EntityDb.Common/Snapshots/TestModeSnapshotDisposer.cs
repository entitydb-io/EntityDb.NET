using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Snapshots
{
    internal class TestModeSnapshotDisposer
    {
        private readonly List<Guid> _entityIds = new();
        private uint _holds;

        public void Hold()
        {
            _holds += 1;
        }

        public void AddEntityId(Guid entityId)
        {
            _entityIds.Add(entityId);
        }

        public void RemoveEntityIds(Guid[] entityIds)
        {
            _entityIds.RemoveAll(entityIds.Contains);
        }

        public void Release()
        {
            _holds -= 1;
        }

        public bool NoHolds(out Guid[] entityIds)
        {
            if (_holds > 0)
            {
                entityIds = Array.Empty<Guid>();

                return false;
            }

            entityIds = _entityIds.ToArray();

            return true;
        }
    }
}
