using EntityDb.Abstractions;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.States;

namespace EntityDb.Common.Sources.Annotations;

internal sealed record AnnotatedSourceData<TData>
(
    Id SourceId,
    TimeStamp SourceTimeStamp,
    Id[] MessageIds,
    TData Data,
    StatePointer[] StatePointers
) : IAnnotatedSourceData<TData>
{
    public static IAnnotatedSourceData<TData> CreateFromBoxedData
    (
        Id sourceId,
        TimeStamp sourceTimeStamp,
        Id[] messageIds,
        object boxedData,
        StatePointer[] statePointers
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
