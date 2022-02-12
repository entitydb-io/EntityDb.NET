using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Tests.Implementations.Entities;
using System;

namespace EntityDb.Common.Tests.Implementations.Strategies
{
    public class TransactionEntityConstructingStrategy : IConstructingStrategy<TransactionEntity>
    {
        public TransactionEntity Construct(Guid entityId)
        {
            return new TransactionEntity();
        }
    }
}
