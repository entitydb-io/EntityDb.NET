using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Documents;

internal interface IMessageDocument<out TData> : IDocument<TData>
{
    Id MessageId { get; }
    Pointer EntityPointer { get; }
}
