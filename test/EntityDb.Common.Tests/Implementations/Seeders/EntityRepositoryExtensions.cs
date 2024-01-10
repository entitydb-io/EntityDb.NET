using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Tests.Implementations.Entities.Deltas;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class EntityRepositoryExtensions
{
    public static void Seed<TEntity>
    (
        this IMultipleEntityRepository<TEntity> entityRepository,
        Id entityId,
        ulong numDeltas
    )
        where TEntity : class, IEntity<TEntity>
    {
        for (ulong i = 0; i < numDeltas; i++)
        {
            entityRepository.Append(entityId, new DoNothing());
        }
    }
}
