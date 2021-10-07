using EntityDb.Abstractions.Commands;
using EntityDb.TestImplementations.Entities;

namespace EntityDb.TestImplementations.Commands
{
    public record DoNothing : ICommand<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            return entity with { VersionNumber = entity.VersionNumber + 1 };
        }
    }
}
