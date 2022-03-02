using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using System;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions;

internal sealed record Transaction : ITransaction
{
    public Guid Id { get; init; }
    public DateTime TimeStamp { get; init; }
    public object AgentSignature { get; init; } = default!;
    public ImmutableArray<ITransactionStep> Steps { get; init; }
}
