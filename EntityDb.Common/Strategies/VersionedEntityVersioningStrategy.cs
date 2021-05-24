using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Entities;
using EntityDb.Common.Facts;

namespace EntityDb.Common.Strategies
{
    internal sealed class VersionedEntityVersioningStrategy<TEntity> : IVersioningStrategy<TEntity>
        where TEntity : IVersionedEntity<TEntity>
    {
        public ulong GetVersionNumber(TEntity entity)
        {
            return entity.VersionNumber;
        }

        public IFact<TEntity> GetVersionNumberFact(ulong versionNumber)
        {
            return new VersionNumberSet<TEntity>(versionNumber);
        }
    }
}
