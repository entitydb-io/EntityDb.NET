using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Documents;

internal interface IMessageDataDocument<out TData> : IDocument<TData>
{
    Id MessageId { get; }
    Pointer StatePointer { get; }
}
