using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.Implementations.Snapshots;

public interface ISnapshotWithTestLogic<TSnapshot> : ISnapshot<TSnapshot>
    where TSnapshot : class
{
    Pointer Pointer { get; }
    static abstract string MongoDbCollectionName { get; }
    static abstract string RedisKeyNamespace { get; }
    static abstract AsyncLocal<Func<TSnapshot, bool>?> ShouldRecordLogic { get; }
    static abstract AsyncLocal<Func<TSnapshot, TSnapshot?, bool>?> ShouldRecordAsLatestLogic { get; }
    TSnapshot WithVersion(Version version);
}