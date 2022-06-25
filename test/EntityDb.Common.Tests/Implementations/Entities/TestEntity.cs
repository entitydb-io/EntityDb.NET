using EntityDb.Common.Entities;
using System;
using System.Linq;
using System.Threading;
using EntityDb.Abstractions.Reducers;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Snapshots;

namespace EntityDb.Common.Tests.Implementations.Entities;

public record TestEntity
(
    Id Id,
    VersionNumber VersionNumber = default
)
: IEntity<TestEntity>, ISnapshot<TestEntity>, IEntityWithVersionNumber<TestEntity>, ISnapshotWithVersionNumber<TestEntity>, ISnapshotWithShouldReplaceLogic<TestEntity>
{
    public const string MongoCollectionName = "Test";
    public const string RedisKeyNamespace = "test-entity";

    public static TestEntity Construct(Id entityId)
    {
        return new TestEntity(entityId);
    }
    
    public static TestEntity Construct(Id entityId, VersionNumber versionNumber)
    {
        return new TestEntity(entityId, versionNumber);
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

    public static AsyncLocal<Func<TestEntity, TestEntity?, bool>?> ShouldReplaceLogic { get; } = new();

    public bool ShouldReplace(TestEntity? previousSnapshot)
    {
        if (ShouldReplaceLogic.Value is not null)
        {
            return ShouldReplaceLogic.Value.Invoke(this, previousSnapshot);
        }
        
        return !Equals(previousSnapshot);
    }
}