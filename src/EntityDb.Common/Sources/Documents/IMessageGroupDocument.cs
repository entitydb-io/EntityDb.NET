using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Documents;

internal interface IMessageGroupDocument<out TData> : IDocument<TData>
{
    Pointer[] EntityPointers { get; }
}
