using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Annotations;

internal sealed record AnnotatedMessageData<TData>
(
    Id SourceId,
    TimeStamp SourceTimeStamp,
    Id MessageId,
    TData Data,
    Pointer StatePointer
) : IAnnotatedMessageData<TData>
{
    public static IAnnotatedMessageData<TData> CreateFromBoxedData
    (
        Id sourceId,
        TimeStamp sourceTimeStamp,
        Id messageId,
        object boxedData,
        Pointer statePointer
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
