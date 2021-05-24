using EntityDb.Abstractions.Strategies;

namespace EntityDb.TestImplementations.Entities
{
    public class TransactionEntityConstructingStrategy : IConstructingStrategy<TransactionEntity>
    {
        public TransactionEntity Construct()
        {
            return new TransactionEntity();
        }
    }
}
