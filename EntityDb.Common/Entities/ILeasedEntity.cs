using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Leases;
using System.Collections.Generic;

namespace EntityDb.Common.Entities
{
    /// <summary>
    /// Represents a type that can be used for an implementation of <see cref="ILeasingStrategy{TEntity}"/>.
    /// </summary>
    public interface ILeasedEntity
    {
        /// <inheritdoc cref="ILeasingStrategy{TEntity}.GetLeases(TEntity)"/>
        public IEnumerable<ILease> GetLeases();
    }
}
