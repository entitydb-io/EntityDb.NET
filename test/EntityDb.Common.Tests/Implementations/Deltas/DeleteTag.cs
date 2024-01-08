using EntityDb.Abstractions.Entities.Deltas;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Common.Tests.Implementations.Entities;

namespace EntityDb.Common.Tests.Implementations.Deltas;

public record DeleteTag(ITag Tag) : DoNothing, IDeleteTagsDelta<TestEntity>
{
    public IEnumerable<ITag> GetTags(TestEntity entity)
    {
        yield return Tag;
    }
}