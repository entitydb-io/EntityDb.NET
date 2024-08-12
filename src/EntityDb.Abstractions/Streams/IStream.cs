using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Abstractions.Streams;

public interface IStream
{
    IStateKey Key { get; }
    Id Id { get; }
}
