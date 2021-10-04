using EntityDb.Abstractions.Leases;

namespace EntityDb.Abstractions.Strategies
{
    /// <summary>
    /// Represents a type used to get leases for a <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be leased.</typeparam>
    public interface ILeasingStrategy<in TEntity>
    {
        /// <summary>
        /// Returns the leases for a <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The leases for <paramref name="entity"/>.</returns>
        ILease[] GetLeases(TEntity entity);
    }
}
