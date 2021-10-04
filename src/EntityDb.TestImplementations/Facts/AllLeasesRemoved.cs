using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Leases;
using EntityDb.TestImplementations.Entities;
using System;

namespace EntityDb.TestImplementations.Facts
{
    public record AllLeasesRemoved : IFact<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            return entity with { Leases = Array.Empty<ILease>() };
        }
    }
}
