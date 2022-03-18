using EntityDb.Abstractions.Reducers;
using EntityDb.Common.Tests.Implementations.Entities;

namespace EntityDb.Common.Tests.Implementations.Commands;

public record Count(ulong Number) : IReducer<TestEntity>
{
    public TestEntity Reduce(TestEntity entity)
    {
        return entity with
        {
            VersionNumber = entity.VersionNumber.Next()
        };
    }
}