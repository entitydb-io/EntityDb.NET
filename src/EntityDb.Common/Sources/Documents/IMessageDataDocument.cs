using EntityDb.Abstractions;
using EntityDb.Abstractions.States;

namespace EntityDb.Common.Sources.Documents;

internal interface IMessageDataDocument<out TData> : IDocument<TData>
{
    Id MessageId { get; }
    StatePointer StatePointer { get; }
}
