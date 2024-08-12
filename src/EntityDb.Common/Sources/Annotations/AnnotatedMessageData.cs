using EntityDb.Abstractions;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.States;

namespace EntityDb.Common.Sources.Annotations;

internal sealed record AnnotatedMessageData<TData>
(
    Id SourceId,
    TimeStamp SourceTimeStamp,
    Id MessageId,
    TData Data,
    StatePointer StatePointer
) : IAnnotatedMessageData<TData>
{
    public static IAnnotatedMessageData<TData> CreateFromBoxedData
    (
        Id sourceId,
        TimeStamp sourceTimeStamp,
        Id messageId,
        object boxedData,
        StatePointer statePointer
    )
    {
        var dataAnnotationType = typeof(AnnotatedMessageData<>).MakeGenericType(boxedData.GetType());

        return (IAnnotatedMessageData<TData>)Activator.CreateInstance
        (
            dataAnnotationType,
            sourceId,
            sourceTimeStamp,
            messageId,
            boxedData,
            statePointer
        )!;
    }
}
