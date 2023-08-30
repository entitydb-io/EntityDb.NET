using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.Common.Transactions;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class TransactionCommandSeeder
{
    public static IEnumerable<ITransactionCommand> CreateFromCommands<TEntity>(Id entityId, uint numCommands, ulong previousVersionNumberValue = 0)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        var previousVersionNumber = new VersionNumber(previousVersionNumberValue);

        for (var versionNumberOffset = 0; versionNumberOffset < numCommands; versionNumberOffset++)
        {
            previousVersionNumber = previousVersionNumber.Next();

            yield return new TransactionCommand
            {
                EntityId = entityId,
                EntityVersionNumber = previousVersionNumber,
                Data = CommandSeeder.Create()
            };
        }
    }
}
