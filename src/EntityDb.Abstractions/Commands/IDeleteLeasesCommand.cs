using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Commands;

/// <summary>
///     Represents a command that deletes leases.
/// </summary>
/// <typeparam name="TEntity">The type of the entity</typeparam>
public interface IDeleteLeasesCommand<in TEntity>
{
    /// <summary>
    ///     Returns the leases that need to be deleted.
    /// </summary>
    /// <param name="entity">The entity for which leases will be removed.</param>
    /// <returns>The leases that need to be deleted.</returns>
    IEnumerable<ILease> GetLeases(TEntity entity);
}

/// <ignore />
[Obsolete("Please use IDeleteLeasesCommand<TEntity> instead. This will be removed in a future version.", true)]
public interface IDeleteLeasesCommand
{
    /// <ignore />
    IEnumerable<ILease> GetLeases() => throw new NotImplementedException();
}
