using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Documents;

internal interface IMessageGroupDocument<out TData> : IDocument<TData>
{
    Id[] MessageIds { get; }
    Pointer[] EntityPointers { get; }
}
