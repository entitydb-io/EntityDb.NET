using System;
using System.Linq;
using System.Threading;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Snapshots;

namespace EntityDb.Common.Tests.Implementations.Entities;

public record TestEntity
    (
        Id Id,
        VersionNumber VersionNumber = default
    )
    : IEntity<TestEntity>, ISnapshotWithTestLogic<TestEntity>
{
    public static TestEntity Construct(Id entityId)
    {
        return new TestEntity(entityId);
    }

    public Id GetId()
    {
        return Id;
    }

    public VersionNumber GetVersionNumber()
    {
        return VersionNumber;
    }

    public TestEntity Reduce(object command)
    {
        return command switch
        {
            DoNothing doNothing => doNothing.Reduce(this),
            Count count => count.Reduce(this),
            _ => throw new NotSupportedException()
        };
    }

    public bool ShouldRecord()
    {
        return ShouldRecordLogic.Value is not null && ShouldRecordLogic.Value.Invoke(this);
    }

    public bool ShouldRecordAsLatest(TestEntity? previousMostRecentSnapshot)
    {
        return ShouldRecordAsLatestLogic.Value is not null && ShouldRecordAsLatestLogic.Value.Invoke(this, previousMostRecentSnapshot);
    }

    public static string RedisKeyNamespace => "test-entity";

    public TestEntity WithVersionNumber(VersionNumber versionNumber)
    {
        return this with { VersionNumber = versionNumber };
    }

    public static AsyncLocal<Func<TestEntity, bool>?> ShouldRecordLogic { get; } = new();

    public static AsyncLocal<Func<TestEntity, TestEntity?, bool>?> ShouldRecordAsLatestLogic { get; } = new();
}