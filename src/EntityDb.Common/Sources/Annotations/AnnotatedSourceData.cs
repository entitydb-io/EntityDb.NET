using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Annotations;

internal record AnnotatedSourceData<TData>
(
    Id SourceId,
    TimeStamp SourceTimeStamp,
    Id[] MessageIds,
    TData Data,
    Pointer[] StatePointers
) : IAnnotatedSourceData<TData>
{
    public static IAnnotatedSourceData<TData> CreateFromBoxedData
    (
        Id sourceId,
        TimeStamp sourceTimeStamp,
        Id[] messageIds,
        object boxedData,
        Pointer[] statePointers
    )
    {
        var dataAnnotationType = typeof(AnnotatedSourceData<>).MakeGenericType(boxedData.GetType());

        return (IAnnotatedSourceData<TData>)Activator.CreateInstance
        (
            dataAnnotationType,
            sourceId,
            sourceTimeStamp,
            messageIds,
            boxedData,
            statePointers
        )!;
    }
}
