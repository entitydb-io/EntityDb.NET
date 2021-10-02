using EntityDb.Abstractions.Strategies;
using System;

namespace EntityDb.TestImplementations.Entities
{
    public class TransactionEntityConstructingStrategy : IConstructingStrategy<TransactionEntity>
    {
        public TransactionEntity Construct(Guid entityId)
        {
            return new TransactionEntity();
        }
    }
}
