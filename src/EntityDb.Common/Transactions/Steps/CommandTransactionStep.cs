using EntityDb.Abstractions.Transactions.Steps;
using System;

namespace EntityDb.Common.Transactions.Steps;

internal sealed record CommandTransactionStep : ICommandTransactionStep
{
    public Guid EntityId { get; init; }
    public object Command { get; init; } = default!;
    public ulong PreviousEntityVersionNumber { get; init; }
    public ulong NextEntityVersionNumber { get; init; }
}
