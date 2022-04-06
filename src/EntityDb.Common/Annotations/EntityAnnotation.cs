using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using System;

namespace EntityDb.Common.Annotations;

internal record EntityAnnotation<TData>
(
    Id TransactionId,
    TimeStamp TransactionTimeStamp,
    Id EntityId,
    VersionNumber EntityVersionNumber,
    TData Data
) : IEntityAnnotation<TData>
{
    public static IEntityAnnotation<TData> CreateFromBoxedData
    (
        Id transactionId,
        TimeStamp transactionTimeStamp,
        Id entityId,
        VersionNumber entityVersionNumber,
        object boxedData
    )
    {
        var dataAnnotationType = typeof(EntityAnnotation<>).MakeGenericType(boxedData.GetType());

        return (IEntityAnnotation<TData>)Activator.CreateInstance
        (
            dataAnnotationType,
            transactionId,
            transactionTimeStamp,
            entityId,
            entityVersionNumber,
            boxedData
        )!;
    }
}
