using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Documents;

internal interface IMessageDocument<out TData> : IDocument<TData>
{
    Pointer EntityPointer { get; }
}
