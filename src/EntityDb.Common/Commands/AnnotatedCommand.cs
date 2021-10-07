using EntityDb.Abstractions.Commands;
using System;

namespace EntityDb.Common.Commands
{
    internal record AnnotatedCommand<TEntity> : IAnnotatedCommand<TEntity>
    {
        public Guid TransactionId { get; init; }
        public DateTime TransactionTimeStamp { get; init; }
        public Guid EntityId { get; init; }
        public ulong EntityVersionNumber { get; init; }
        public ICommand<TEntity> Command { get; init; } = default!;
    }
}
