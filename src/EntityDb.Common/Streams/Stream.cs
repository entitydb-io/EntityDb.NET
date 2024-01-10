using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Streams;

internal sealed record Stream
{
    public required Key Key { get; init; }
    public required Id Id { get; init; }
    public required bool New { get; init; }
}
