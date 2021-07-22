﻿using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Leases;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Leases;
using System.Collections.Generic;

namespace EntityDb.TestImplementations.Facts
{
    public record Counted(int Number) : IFact<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            var leases = new List<ILease>();

            if (entity.Leases != null)
            {
                leases.AddRange(entity.Leases);
            }

            leases.Add(new CountLease(Number));

            return entity with
            {
                Leases = leases.ToArray(),
            };
        }
    }
}
