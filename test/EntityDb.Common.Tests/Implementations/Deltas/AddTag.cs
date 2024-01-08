using EntityDb.Abstractions.Entities.Deltas;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Common.Tests.Implementations.Entities;

namespace EntityDb.Common.Tests.Implementations.Deltas;

public record AddTag(ITag Tag) : DoNothing, IAddTagsDelta<TestEntity>
{
    public IEnumerable<ITag> GetTags(TestEntity entity)
    {
        yield return Tag;
    }
}