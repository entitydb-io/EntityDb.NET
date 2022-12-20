using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.Common.Transactions;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class TransactionCommandSeeder
{
    public static IEnumerable<ITransactionCommand> CreateFromCommands<TEntity>(Id entityId, uint numCommands)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        for (var previousVersionNumber = new VersionNumber(0);
             previousVersionNumber.Value < numCommands;
             previousVersionNumber = previousVersionNumber.Next())
        {
            var entityVersionNumber = previousVersionNumber.Next();

            yield return new TransactionCommandWithSnapshot
            {
                EntityId = entityId,
                Snapshot = TEntity.Construct(entityId).WithVersionNumber(entityVersionNumber),
                EntityVersionNumber = entityVersionNumber,
                Command = CommandSeeder.Create()
            };
        }
    }
}
