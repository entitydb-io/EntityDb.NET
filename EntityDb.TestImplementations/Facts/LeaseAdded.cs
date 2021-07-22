using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Leases;
using EntityDb.Common.Leases;
using EntityDb.TestImplementations.Entities;
using System.Collections.Generic;

namespace EntityDb.TestImplementations.Facts
{
    public record LeaseAdded(string LeaseScope, string LeaseLabel, string LeaseValue) : IFact<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            var leases = new List<ILease>();

            if (entity.Leases != null)
            {
                leases.AddRange(entity.Leases);
            }

            leases.Add(new Lease(LeaseScope, LeaseLabel, LeaseValue));

            return entity with
            {
                Leases = leases.ToArray(),
            };
        }
    }
}
