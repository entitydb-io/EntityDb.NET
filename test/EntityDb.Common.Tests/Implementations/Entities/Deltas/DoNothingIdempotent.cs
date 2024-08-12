using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.States.Deltas;

namespace EntityDb.Common.Tests.Implementations.Entities.Deltas;

public record DoNothingIdempotent(IMessageKey MessageKey) : DoNothing, IAddMessageKeyDelta
{
    public IMessageKey GetMessageKey()
    {
        return MessageKey;
    }
}
