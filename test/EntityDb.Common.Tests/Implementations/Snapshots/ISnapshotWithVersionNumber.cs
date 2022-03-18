using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Tests.Implementations.Snapshots;

public interface ISnapshotWithVersionNumber<TSnapshot>
{
    static abstract TSnapshot Construct(Id id, VersionNumber versionNumber);
}