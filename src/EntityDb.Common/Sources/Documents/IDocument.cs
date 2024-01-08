using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Documents;

internal interface IDocument<out TData>
{
    Id SourceId { get; }
    TimeStamp SourceTimeStamp { get; }
    TData Data { get; }
}
