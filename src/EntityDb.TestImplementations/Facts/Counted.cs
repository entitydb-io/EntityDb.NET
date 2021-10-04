using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Leases;
using EntityDb.TestImplementations.Tags;
using System.Collections.Generic;

namespace EntityDb.TestImplementations.Facts
{
    public record Counted(int Number) : IFact<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            List<ILease>? leases = new List<ILease>();

            if (entity.Leases != null)
            {
                leases.AddRange(entity.Leases);
            }

            leases.Add(new CountLease(Number));

            List<ITag>? tags = new List<ITag>();

            if (entity.Tags != null)
            {
                tags.AddRange(entity.Tags);
            }

            tags.Add(new CountTag(Number));

            return entity with { Leases = leases.ToArray(), Tags = tags.ToArray() };
        }
    }
}
