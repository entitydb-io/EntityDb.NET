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
: IEntity<TestEntity>, ISnapshotWithTestMethods<TestEntity>
{
    public const string MongoCollectionName = "Test";
    public const string RedisKeyNamespace = "test-entity";

    public static TestEntity Construct(Id entityId)
    {
        return new TestEntity(entityId);
    }
    
    public TestEntity WithVersionNumber(VersionNumber versionNumber)
    {
        return this with { VersionNumber = versionNumber };
    }

    public Id GetId()
    {
        return Id;
    }

    public VersionNumber GetVersionNumber()
    {
        return VersionNumber;
    }

    public TestEntity Reduce(object[] commands)
    {
        return commands.Aggregate(this, (previousEntity, nextCommand) => nextCommand switch
        {
            DoNothing doNothing => doNothing.Reduce(previousEntity),
            Count count => count.Reduce(previousEntity),
            _ => throw new NotSupportedException()
        });
    }

    public static AsyncLocal<Func<TestEntity, bool>?> ShouldRecordLogic { get; } = new();

    public bool ShouldRecord()
    {
        if (ShouldRecordLogic.Value is not null)
        {
            return ShouldRecordLogic.Value.Invoke(this);
        }

        return false;
    }

    public static AsyncLocal<Func<TestEntity, TestEntity?, bool>?> ShouldRecordAsMostRecentLogic { get; } = new();

    public bool ShouldRecordAsLatest(TestEntity? previousMostRecentSnapshot)
    {
        if (ShouldRecordAsMostRecentLogic.Value is not null)
        {
            return ShouldRecordAsMostRecentLogic.Value.Invoke(this, previousMostRecentSnapshot);
        }
        
        return !Equals(previousMostRecentSnapshot);
    }
}