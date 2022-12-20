using EntityDb.Abstractions.Transactions;

namespace EntityDb.Common.Transactions;

internal interface ITransactionCommandWithSnapshot : ITransactionCommand
{
    object Snapshot { get; }
}
