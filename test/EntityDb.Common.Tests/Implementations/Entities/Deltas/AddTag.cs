using EntityDb.Abstractions.States.Attributes;
using EntityDb.Abstractions.States.Deltas;

namespace EntityDb.Common.Tests.Implementations.Entities.Deltas;

public record AddTag(ITag Tag) : DoNothing, IAddTagsDelta<TestEntity>
{
    public IEnumerable<ITag> GetTags(TestEntity state)
    {
        yield return Tag;
    }
}
