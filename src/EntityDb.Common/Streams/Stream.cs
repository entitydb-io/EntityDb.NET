using EntityDb.Abstractions;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.Streams;

namespace EntityDb.Common.Streams;

internal sealed class Stream : IStream
{
    public required bool IsNew { get; set; }
    public required IStateKey Key { get; init; }
    public required Id Id { get; init; }
}
