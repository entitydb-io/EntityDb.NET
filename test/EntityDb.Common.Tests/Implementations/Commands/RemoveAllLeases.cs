using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Common.Tests.Implementations.Entities;
using System;

namespace EntityDb.Common.Tests.Implementations.Commands
{
    public record RemoveAllLeases : ICommand<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            return entity with { VersionNumber = entity.VersionNumber + 1, Leases = Array.Empty<ILease>() };
        }
    }
}
