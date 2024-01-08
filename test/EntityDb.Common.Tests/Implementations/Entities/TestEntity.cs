using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Snapshots.Transforms;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.Implementations.Entities;

public record TestEntity : IEntity<TestEntity>, ISnapshotWithTestLogic<TestEntity>
{
    public static TestEntity Construct(Pointer pointer)
    {
        return new TestEntity
        {
            Pointer = pointer,
        };
    }

    public Pointer GetPointer()
    {
        return Pointer;
    }

    public static bool CanReduce(object delta)
    {
        return delta is IReducer<TestEntity>;
    }

    public TestEntity Reduce(object delta)
    {
        if (delta is IReducer<TestEntity> reducer) return reducer.Reduce(this);

        throw new NotSupportedException();
    }

    public bool ShouldRecord()
    {
        return ShouldRecordLogic.Value is not null && ShouldRecordLogic.Value.Invoke(this);
    }

    public bool ShouldRecordAsLatest(TestEntity? previousMostRecentSnapshot)
    {
        return ShouldRecordAsLatestLogic.Value is not null &&
               ShouldRecordAsLatestLogic.Value.Invoke(this, previousMostRecentSnapshot);
    }

    public required Pointer Pointer { get; init; }

    public static string MongoDbCollectionName => "TestEntities";
    public static string RedisKeyNamespace => "test-entity";

    public TestEntity WithVersion(Version version)
    {
        return new TestEntity { Pointer = Pointer.Id + version };
    }

    public static AsyncLocal<Func<TestEntity, bool>?> ShouldRecordLogic { get; } = new();

    public static AsyncLocal<Func<TestEntity, TestEntity?, bool>?> ShouldRecordAsLatestLogic { get; } = new();
}