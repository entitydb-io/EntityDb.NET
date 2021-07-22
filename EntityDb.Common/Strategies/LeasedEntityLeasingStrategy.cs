using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Leases;
using EntityDb.Common.Entities;
using System.Linq;

namespace EntityDb.Common.Strategies
{
    internal sealed class LeasedEntityLeasingStrategy<TEntity> : ILeasingStrategy<TEntity>
        where TEntity : ILeasedEntity
    {
        public ILease[] GetLeases(TEntity entity)
        {
            return entity.GetLeases().ToArray();
        }
    }
}
