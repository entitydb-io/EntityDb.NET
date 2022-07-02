using System;
using System.Threading;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Tests.Implementations.Snapshots;

public interface ISnapshotWithTestLogic<TSnapshot> : ISnapshot<TSnapshot>
{
    VersionNumber VersionNumber { get; }
    TSnapshot WithVersionNumber(VersionNumber versionNumber);
    static abstract string RedisKeyNamespace { get; }
    static abstract AsyncLocal<Func<TSnapshot, bool>?> ShouldRecordLogic { get; }
    static abstract AsyncLocal<Func<TSnapshot, TSnapshot?, bool>?> ShouldRecordAsLatestLogic { get; }
}