using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.TestImplementations.Entities;
using System;

namespace EntityDb.TestImplementations.Commands
{
    public record RemoveAllLeases : ICommand<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            return entity with { VersionNumber = entity.VersionNumber + 1, Leases = Array.Empty<ILease>() };
        }
    }
}
