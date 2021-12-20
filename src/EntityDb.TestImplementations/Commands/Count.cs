using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Leases;
using EntityDb.TestImplementations.Tags;
using System.Collections.Generic;

namespace EntityDb.TestImplementations.Commands
{
    public record Count(int Number) : ICommand<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            var leases = new List<ILease>();

            if (entity.Leases != null)
            {
                leases.AddRange(entity.Leases);
            }

            leases.Add(new CountLease(Number));

            var tags = new List<ITag>();

            if (entity.Tags != null)
            {
                tags.AddRange(entity.Tags);
            }

            tags.Add(new CountTag(Number));

            return entity with
            {
                VersionNumber = entity.VersionNumber + 1, Leases = leases.ToArray(), Tags = tags.ToArray()
            };
        }
    }
}
