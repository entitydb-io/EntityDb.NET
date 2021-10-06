using EntityDb.Abstractions.Strategies;
using EntityDb.TestImplementations.Entities;
using System;

namespace EntityDb.TestImplementations.Strategies
{
    public class TransactionEntityConstructingStrategy : IConstructingStrategy<TransactionEntity>
    {
        public TransactionEntity Construct(Guid entityId)
        {
            return new TransactionEntity();
        }
    }
}
