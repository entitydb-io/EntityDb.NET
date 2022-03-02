using EntityDb.Abstractions.Transactions.Steps;

namespace EntityDb.Common.Transactions.Steps;

internal sealed record AppendCommandTransactionStep : TransactionStepBase, IAppendCommandTransactionStep
{
    public object Command { get; init; } = default!;
    public ulong PreviousEntityVersionNumber { get; init; }
}
