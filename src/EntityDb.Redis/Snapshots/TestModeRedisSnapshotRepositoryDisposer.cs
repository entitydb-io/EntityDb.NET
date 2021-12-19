using System;
using System.Collections.Generic;

namespace EntityDb.Redis.Snapshots
{
    internal class TestModeRedisSnapshotRepositoryDisposer
    {
        private uint _locks;
        private readonly List<Guid> _disposeIds = new();

        public void Lock()
        {
            lock (this)
            {
                _locks += 1;
            }
        }

        public void AddDisposeId(Guid disposeId)
        {
            lock (this)
            {
                _disposeIds.Add(disposeId);
            }
        }

        public void Release()
        {
            lock (this)
            {
                _locks -= 1;
            }
        }

        public bool TryDispose(out Guid[] disposeIds)
        {
            lock (this)
            {
                if (_locks > 0)
                {
                    disposeIds = Array.Empty<Guid>();
                    return false;
                }

                disposeIds = _disposeIds.ToArray();

                _disposeIds.Clear();

                return true;
            }
        }
    }
}
