using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Annotations;

internal record EntityAnnotation<TData>
(
    Id TransactionId,
    TimeStamp TransactionTimeStamp,
    Id EntityId,
    VersionNumber EntityVersionNumber,
    TData Data
) : IEntityAnnotation<TData>;
