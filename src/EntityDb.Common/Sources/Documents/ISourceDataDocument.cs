using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Documents;

internal interface ISourceDataDocument<out TData> : IDocument<TData>
{
    Id[] MessageIds { get; }
    Pointer[] StatePointers { get; }
}
