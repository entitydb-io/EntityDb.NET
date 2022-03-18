using EntityDb.Common.Entities;
using System;
using EntityDb.Abstractions.Reducers;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Tests.Implementations.Entities;

public record TestEntity
(
    VersionNumber VersionNumber = default
)
: IEntity<TestEntity>, ISnapshot<TestEntity>
{
    public const string MongoCollectionName = "Test";
    public const string RedisKeyNamespace = "test-entity";

    public static TestEntity Construct(Id entityId)
    {
        return new TestEntity();
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
    
    public bool ShouldReplace(TestEntity? previousSnapshot)
    {
        return !Equals(previousSnapshot);
    }
}