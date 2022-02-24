using EntityDb.Abstractions.Annotations;
using System;

namespace EntityDb.Common.Annotations;

internal record EntityAnnotation<TData>
(
    Guid TransactionId,
    DateTime TransactionTimeStamp,
    Guid EntityId,
    ulong EntityVersionNumber,
    TData Data
) : IEntityAnnotation<TData>;
