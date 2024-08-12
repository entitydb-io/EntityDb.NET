using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.States.Deltas;
using EntityDb.Abstractions.States.Transforms;
using EntityDb.Common.Tests.Implementations.Projections;
using EntityDb.Common.Tests.Implementations.Sources.Attributes;

namespace EntityDb.Common.Tests.Implementations.Entities.Deltas;

public sealed record StoreNumber(ulong Number) : IReducer<TestEntity>, IMutator<OneToOneProjection>,
    IAddLeasesDelta<TestEntity>, IAddTagsDelta<TestEntity>
{
    public IEnumerable<ILease> GetLeases(TestEntity state)
    {
        yield return new CountLease(Number);
    }

    public IEnumerable<ITag> GetTags(TestEntity state)
    {
        yield return new CountTag(Number);
    }

    public void Mutate(OneToOneProjection projection)
    {
        projection.StatePointer = projection.StatePointer.Next();
    }

    public TestEntity Reduce(TestEntity entity)
    {
        return new TestEntity { StatePointer = entity.StatePointer.Next() };
    }
}
