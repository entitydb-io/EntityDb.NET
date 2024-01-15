using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Abstractions.States.Deltas;

/// <summary>
///     Represents a delta that deletes leases.
/// </summary>
/// <typeparam name="TState">The type of the state</typeparam>
public interface IDeleteLeasesDelta<in TState>
{
    /// <summary>
    ///     Returns the leases that need to be deleted.
    /// </summary>
    /// <param name="state">The state for which leases will be removed.</param>
    /// <returns>The leases that need to be deleted.</returns>
    IEnumerable<ILease> GetLeases(TState state);
}
