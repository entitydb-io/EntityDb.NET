using EntityDb.Abstractions.Commands;
using EntityDb.TestImplementations.Entities;

namespace EntityDb.TestImplementations.Commands
{
    public record SetRole(string Role) : ICommand<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            return entity with { VersionNumber = entity.VersionNumber + 1, Role = Role };
        }
    }
}
