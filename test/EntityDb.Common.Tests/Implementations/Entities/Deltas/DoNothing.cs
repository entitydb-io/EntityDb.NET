using EntityDb.Abstractions.States.Transforms;
using EntityDb.Common.Tests.Implementations.Projections;

namespace EntityDb.Common.Tests.Implementations.Entities.Deltas;

public record DoNothing : IReducer<TestEntity>, IMutator<OneToOneProjection>
{
    public void Mutate(OneToOneProjection projection)
    {
        projection.Pointer = projection.Pointer.Next();
    }

    public TestEntity Reduce(TestEntity entity)
    {
        return new TestEntity { Pointer = entity.Pointer.Next() };
    }
}
