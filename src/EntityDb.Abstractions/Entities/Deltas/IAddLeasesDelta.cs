using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Abstractions.Entities.Deltas;

/// <summary>
///     Represents a delta that adds leases.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IAddLeasesDelta<in TEntity>
    where TEntity : IEntity<TEntity>
{
    /// <summary>
    ///     Returns the leases that need to be added.
    /// </summary>
    /// <param name="entity">The entity for which leases will be added.</param>
    /// <returns>The leases that need to be added.</returns>
    IEnumerable<ILease> GetLeases(TEntity entity);
}
