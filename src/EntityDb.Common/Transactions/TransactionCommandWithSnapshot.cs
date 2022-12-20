namespace EntityDb.Common.Transactions;

internal record TransactionCommandWithSnapshot : TransactionCommand, ITransactionCommandWithSnapshot
{
    public object Snapshot { get; init; } = default!;
}
