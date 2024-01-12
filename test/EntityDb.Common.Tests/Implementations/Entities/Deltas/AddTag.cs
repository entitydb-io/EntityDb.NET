using EntityDb.Abstractions.States.Attributes;
using EntityDb.Abstractions.States.Deltas;

namespace EntityDb.Common.Tests.Implementations.Entities.Deltas;

public sealed record AddTag(ITag Tag) : DoNothing, IAddTagsDelta<TestEntity>
{
    public IEnumerable<ITag> GetTags(TestEntity state)
    {
        yield return Tag;
    }
}
