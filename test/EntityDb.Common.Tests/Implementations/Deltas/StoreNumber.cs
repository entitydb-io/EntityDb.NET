using EntityDb.Abstractions.Entities.Deltas;
using EntityDb.Abstractions.Snapshots.Transforms;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Tests.Implementations.Leases;
using EntityDb.Common.Tests.Implementations.Projections;
using EntityDb.Common.Tests.Implementations.Tags;

namespace EntityDb.Common.Tests.Implementations.Deltas;

public record StoreNumber(ulong Number) : IReducer<TestEntity>, IMutator<OneToOneProjection>,
    IAddLeasesDelta<TestEntity>, IAddTagsDelta<TestEntity>
{
    public IEnumerable<ILease> GetLeases(TestEntity entity)
    {
        yield return new CountLease(Number);
    }

    public IEnumerable<ITag> GetTags(TestEntity entity)
    {
        yield return new CountTag(Number);
    }

    public void Mutate(OneToOneProjection projection)
    {
        projection.Pointer = projection.Pointer.Next();
    }

    public TestEntity Reduce(TestEntity entity)
    {
        return new TestEntity { Pointer = entity.Pointer.Next() };
    }
}