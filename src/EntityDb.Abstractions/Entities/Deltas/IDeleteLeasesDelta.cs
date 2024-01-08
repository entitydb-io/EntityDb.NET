using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Abstractions.Entities.Deltas;

/// <summary>
///     Represents a delta that deletes leases.
/// </summary>
/// <typeparam name="TEntity">The type of the entity</typeparam>
public interface IDeleteLeasesDelta<in TEntity>
    where TEntity : IEntity<TEntity>
{
    /// <summary>
    ///     Returns the leases that need to be deleted.
    /// </summary>
    /// <param name="entity">The entity for which leases will be removed.</param>
    /// <returns>The leases that need to be deleted.</returns>
    IEnumerable<ILease> GetLeases(TEntity entity);
}
