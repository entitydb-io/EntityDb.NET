using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;
using EntityDb.EntityFramework.Snapshots;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityDb.Common.Tests.Implementations.Snapshots;

public interface ISnapshotWithTestLogic<TSnapshot> : ISnapshot<TSnapshot>, IEntityFrameworkSnapshot<TSnapshot>
    where TSnapshot : class
{
    VersionNumber VersionNumber { get; }
    static abstract string MongoDbCollectionName { get; }
    static abstract string RedisKeyNamespace { get; }
    static abstract AsyncLocal<Func<TSnapshot, bool>?> ShouldRecordLogic { get; }
    static abstract AsyncLocal<Func<TSnapshot, TSnapshot?, bool>?> ShouldRecordAsLatestLogic { get; }
    static abstract void Configure(EntityTypeBuilder<TSnapshot> snapshotBuilder);
    TSnapshot WithVersionNumber(VersionNumber versionNumber);
}