using EntityDb.Abstractions.Leases;

namespace EntityDb.Abstractions.Commands;

/// <summary>
///     Represents a command that adds leases.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IAddLeasesCommand<in TEntity>
{
    /// <summary>
    ///     Returns the leases that need to be added.
    /// </summary>
    /// <param name="entity">The entity for which leases will be added.</param>
    /// <returns>The leases that need to be added.</returns>
    IEnumerable<ILease> GetLeases(TEntity entity);
}

/// <ignore />
[Obsolete("Please use IAddLeasesCommand<TEntity> instead. This will be removed in a future version.", true)]
public interface IAddLeasesCommand
{
    /// <ignore />
    IEnumerable<ILease> GetLeases() => throw new NotImplementedException();
}
