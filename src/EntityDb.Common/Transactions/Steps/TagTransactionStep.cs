using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions.Steps;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions.Steps;

internal sealed record AddTagsTransactionStep : TransactionStepBase, IAddTagsTransactionStep
{
    public ImmutableArray<ITag> Tags { get; init; }
}
