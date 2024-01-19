using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Abstractions.States.Deltas;

public interface IAddMessageKeyDelta
{
    IMessageKey? GetMessageKey();
}
