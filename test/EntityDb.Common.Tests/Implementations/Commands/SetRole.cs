using EntityDb.Abstractions.Commands;
using EntityDb.Common.Tests.Implementations.Entities;

namespace EntityDb.Common.Tests.Implementations.Commands
{
    public record SetRole(string Role) : ICommand<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            return entity with { VersionNumber = entity.VersionNumber + 1, Role = Role };
        }
    }
}
