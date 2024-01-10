using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class MessageSeeder
{
    public static IEnumerable<Message> CreateFromDeltas<TEntity>(Id entityId, uint numDeltas,
        ulong previousVersionValue = 0)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        var previousVersion = new Version(previousVersionValue);

        for (var versionOffset = 0; versionOffset < numDeltas; versionOffset++)
        {
            previousVersion = previousVersion.Next();

            yield return new Message
            {
                Id = Id.NewId(),
                EntityPointer = entityId + previousVersion,
                Delta = DeltaSeeder.Create(),
            };
        }
    }
}