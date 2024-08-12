using EntityDb.Abstractions;
using EntityDb.Abstractions.States;

namespace EntityDb.Common.Sources.Documents;

internal interface ISourceDataDocument<out TData> : IDocument<TData>
{
    Id[] MessageIds { get; }
    StatePointer[] StatePointers { get; }
}
