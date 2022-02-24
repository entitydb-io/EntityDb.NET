using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Transactions.Steps;
using System;

namespace EntityDb.Common.Transactions.Steps
{
    internal sealed record CommandTransactionStep<TEntity> : ICommandTransactionStep<TEntity>
    {
        public Guid EntityId { get; init; }
        public ICommand<TEntity> Command { get; init; } = default!;
        public TEntity PreviousEntitySnapshot { get; init; } = default!;
        public ulong PreviousEntityVersionNumber { get; init; }
        public TEntity NextEntitySnapshot { get; init; } = default!;
        public ulong NextEntityVersionNumber { get; init; }
    }
}
