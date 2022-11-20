using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Tests.Implementations.Snapshots;

public interface ISnapshotWithTestLogic<TSnapshot> : ISnapshot<TSnapshot>
{
    Guid IdValue { get; }
    ulong VersionNumberValue { get; }

    VersionNumber VersionNumber { get; }
    static abstract string RedisKeyNamespace { get; }
    static abstract AsyncLocal<Func<TSnapshot, bool>?> ShouldRecordLogic { get; }
    static abstract AsyncLocal<Func<TSnapshot, TSnapshot?, bool>?> ShouldRecordAsLatestLogic { get; }
    TSnapshot WithVersionNumber(VersionNumber versionNumber);
}