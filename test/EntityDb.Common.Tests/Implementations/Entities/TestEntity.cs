using EntityDb.Common.Entities;
using System;
using System.Threading;
using EntityDb.Abstractions.Reducers;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;
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
        var newEntity = this;

        foreach (var command in commands)
        {
            if (command is not IReducer<TestEntity> reducer)
            {
                throw new NotImplementedException();
            }
            
            newEntity = reducer.Reduce(newEntity);
        }

        return newEntity;
    }

    public static AsyncLocal<Func<TestEntity, TestEntity?, bool>?> ShouldReplaceLogic { get; } = new();

    public bool ShouldReplace(TestEntity? previousSnapshot)
    {
        if (ShouldReplaceLogic.Value != null)
        {
            return ShouldReplaceLogic.Value.Invoke(this, previousSnapshot);
        }
        
        return !Equals(previousSnapshot);
    }
}