using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using System;

namespace EntityDb.Common.Transactions.Steps;

internal sealed record TagTransactionStep<TEntity> : ITagTransactionStep<TEntity>
{
    public Guid EntityId { get; init; }
    public ulong TaggedAtEntityVersionNumber { get; init; }
    public ITransactionMetaData<ITag> Tags { get; init; } = default!;
}
