using EntityDb.Abstractions.Transactions;
using System;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions
{
    internal sealed record Transaction<TEntity> : ITransaction<TEntity>
    {
        public Guid Id { get; init; }
        public DateTime TimeStamp { get; init; }
        public object AgentSignature { get; init; } = default!;
        public ImmutableArray<ITransactionStep<TEntity>> Steps { get; init; }
    }
}
