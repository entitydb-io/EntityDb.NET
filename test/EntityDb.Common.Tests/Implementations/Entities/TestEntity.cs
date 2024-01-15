using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.States;
using EntityDb.Abstractions.States.Transforms;
using EntityDb.Common.Tests.Implementations.States;

namespace EntityDb.Common.Tests.Implementations.Entities;

public sealed record TestEntity : IEntity<TestEntity>, IStateWithTestLogic<TestEntity>
{
    public required StatePointer StatePointer { get; init; }

    public static TestEntity Construct(StatePointer statePointer)
    {
        return new TestEntity { StatePointer = statePointer };
    }

    public StatePointer GetPointer()
    {
        return StatePointer;
    }

    public static bool CanReduce(object delta)
    {
        return delta is IReducer<TestEntity>;
    }

    public TestEntity Reduce(object delta)
    {
        if (delta is IReducer<TestEntity> reducer)
        {
            return reducer.Reduce(this);
        }

        throw new NotSupportedException();
    }

    public bool ShouldRecord()
    {
        return ShouldRecordLogic.Value is not null && ShouldRecordLogic.Value.Invoke(this);
    }

    public bool ShouldRecordAsLatest(TestEntity? previousLatestState)
    {
        return ShouldRecordAsLatestLogic.Value is not null &&
               ShouldRecordAsLatestLogic.Value.Invoke(this, previousLatestState);
    }

    public static string MongoDbCollectionName => "TestEntities";
    public static string RedisKeyNamespace => "test-entity";

    public static AsyncLocal<Func<TestEntity, bool>?> ShouldRecordLogic { get; } = new();

    public static AsyncLocal<Func<TestEntity, TestEntity?, bool>?> ShouldRecordAsLatestLogic { get; } = new();

    public TestEntity WithVersion(StateVersion stateVersion)
    {
        return new TestEntity { StatePointer = StatePointer.Id + stateVersion };
    }
}
