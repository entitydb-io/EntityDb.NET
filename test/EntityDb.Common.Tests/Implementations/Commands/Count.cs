using EntityDb.Abstractions.Reducers;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Tests.Implementations.Projections;

namespace EntityDb.Common.Tests.Implementations.Commands;

public record Count(ulong Number) : IReducer<TestEntity>, IReducer<OneToOneProjection>
{
    public TestEntity Reduce(TestEntity entity)
    {
        return entity with
        {
            VersionNumber = entity.VersionNumber.Next()
        };
    }

    public OneToOneProjection Reduce(OneToOneProjection projection)
    {
        return projection with
        {
            VersionNumber = projection.VersionNumber.Next(),
        };
    }
}