using EntityDb.Abstractions.Facts;
using EntityDb.TestImplementations.Entities;

namespace EntityDb.TestImplementations.Facts
{
    public record RoleSet(string Role) : IFact<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            return entity with { Role = Role };
        }
    }
}
