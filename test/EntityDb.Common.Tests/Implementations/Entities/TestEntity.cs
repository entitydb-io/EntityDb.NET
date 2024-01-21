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

    public bool ShouldPersist()
    {
        return ShouldPersistLogic.Value is not null && ShouldPersistLogic.Value.Invoke(this);
    }

    public bool ShouldPersistAsLatest()
    {
        return ShouldPersistAsLatestLogic.Value is not null &&
               ShouldPersistAsLatestLogic.Value.Invoke(this);
    }

    public static string MongoDbCollectionName => "TestEntities";
    public static string RedisKeyNamespace => "test-entity";

    public static AsyncLocal<Func<TestEntity, bool>?> ShouldPersistLogic { get; } = new();

    public static AsyncLocal<Func<TestEntity, bool>?> ShouldPersistAsLatestLogic { get; } = new();
}
