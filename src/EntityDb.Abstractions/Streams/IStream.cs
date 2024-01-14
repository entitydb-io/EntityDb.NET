using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Streams;

public interface IStream
{
    Key Key { get; }
    Id Id { get; }
}
