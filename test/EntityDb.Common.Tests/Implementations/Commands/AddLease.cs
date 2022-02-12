using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Common.Leases;
using EntityDb.Common.Tests.Implementations.Entities;
using System.Collections.Generic;

namespace EntityDb.Common.Tests.Implementations.Commands
{
    public record AddLease(string LeaseScope, string LeaseLabel, string LeaseValue) : ICommand<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            var leases = new List<ILease>();

            if (entity.Leases != null)
            {
                leases.AddRange(entity.Leases);
            }

            leases.Add(new Lease(LeaseScope, LeaseLabel, LeaseValue));

            return entity with { VersionNumber = entity.VersionNumber + 1, Leases = leases.ToArray() };
        }
    }
}
