using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Transactions.Steps;

internal sealed record AppendCommandTransactionStep : TransactionStepBase, IAppendCommandTransactionStep
{
    public object Command { get; init; } = default!;
    public VersionNumber PreviousEntityVersionNumber { get; init; }
}
