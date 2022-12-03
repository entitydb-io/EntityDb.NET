using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Annotations;

internal record EntitiesAnnotation<TData>
(
    Id TransactionId,
    TimeStamp TransactionTimeStamp,
    Id[] EntityIds,
    TData Data
) : IEntitiesAnnotation<TData>
{
    public static IEntitiesAnnotation<TData> CreateFromBoxedData
    (
        Id transactionId,
        TimeStamp transactionTimeStamp,
        Id[] entityIds,
        object boxedData
    )
    {
        var dataAnnotationType = typeof(EntitiesAnnotation<>).MakeGenericType(boxedData.GetType());

        return (IEntitiesAnnotation<TData>)Activator.CreateInstance
        (
            dataAnnotationType,
            transactionId,
            transactionTimeStamp,
            entityIds,
            boxedData
        )!;
    }
}
