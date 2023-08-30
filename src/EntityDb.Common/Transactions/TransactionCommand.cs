using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Transactions;

internal record TransactionCommand : ITransactionCommand
{
    public Id EntityId { get; init; }
    public VersionNumber EntityVersionNumber { get; init; }
    public object Data { get; init; } = default!;
}
