using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Entities;

namespace EntityDb.Common.Strategies
{
    internal sealed class VersionedEntityVersioningStrategy<TEntity> : IVersioningStrategy<TEntity>
        where TEntity : IVersionedEntity<TEntity>
    {
        public ulong GetVersionNumber(TEntity entity)
        {
            return entity.VersionNumber;
        }
    }
}
