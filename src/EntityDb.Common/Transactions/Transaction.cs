using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions;

internal sealed record Transaction : ITransaction
{
    public Id Id { get; init; }
    public TimeStamp TimeStamp { get; init; }
    public object AgentSignature { get; init; } = default!;
    public ImmutableArray<ITransactionCommand> Commands { get; init; }
}
