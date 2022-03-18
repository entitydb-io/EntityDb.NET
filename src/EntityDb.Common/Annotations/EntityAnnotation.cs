using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;

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
    public static EntityAnnotation<TData> CreateFrom(ITransaction transaction, ITransactionStep transactionStep,
        TData data)
    {
        return new
        (
            transaction.Id,
            transaction.TimeStamp,
            transactionStep.EntityId,
            transactionStep.EntityVersionNumber,
            data
        );
    }
}
