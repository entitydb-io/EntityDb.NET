using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.States;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Tests.Implementations.Leases;
using EntityDb.Common.Tests.Implementations.Projections;
using EntityDb.Common.Tests.Implementations.Tags;

namespace EntityDb.Common.Tests.Implementations.Commands;

public record StoreNumber(ulong Number) : IReducer<TestEntity>, IMutator<OneToOneProjection>, IAddLeasesCommand<TestEntity>, IAddTagsCommand<TestEntity>
{
    public void Mutate(OneToOneProjection projection)
    {
        projection.VersionNumber = projection.VersionNumber.Next();
    }

    public TestEntity Reduce(TestEntity entity)
    {
        return entity with
        {
            VersionNumber = entity.VersionNumber.Next()
        };
    }

    public IEnumerable<ILease> GetLeases(TestEntity testEntity)
    {
        yield return new CountLease(Number);
    }

    public IEnumerable<ITag> GetTags(TestEntity testEntity)
    {
        yield return new CountTag(Number);
    }
}