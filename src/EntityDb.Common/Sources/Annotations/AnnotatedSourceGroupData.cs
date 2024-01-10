using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Annotations;

internal record AnnotatedSourceGroupData<TData>
(
    Id SourceId,
    TimeStamp SourceTimeStamp,
    Id[] MessageIds,
    TData Data,
    Pointer[] EntityPointers
) : IAnnotatedSourceGroupData<TData>
{
    public static IAnnotatedSourceGroupData<TData> CreateFromBoxedData
    (
        Id sourceId,
        TimeStamp sourceTimeStamp,
        Id[] messageIds,
        object boxedData,
        Pointer[] entityPointers
    )
    {
        var dataAnnotationType = typeof(AnnotatedSourceGroupData<>).MakeGenericType(boxedData.GetType());

        return (IAnnotatedSourceGroupData<TData>)Activator.CreateInstance
        (
            dataAnnotationType,
            sourceId,
            sourceTimeStamp,
            messageIds,
            boxedData,
            entityPointers
        )!;
    }
}
