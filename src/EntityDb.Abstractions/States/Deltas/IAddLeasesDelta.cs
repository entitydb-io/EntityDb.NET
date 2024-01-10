using EntityDb.Abstractions.States.Attributes;

namespace EntityDb.Abstractions.States.Deltas;

/// <summary>
///     Represents a delta that adds leases.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public interface IAddLeasesDelta<in TState>
{
    /// <summary>
    ///     Returns the leases that need to be added.
    /// </summary>
    /// <param name="state">The state for which leases will be added.</param>
    /// <returns>The leases that need to be added.</returns>
    IEnumerable<ILease> GetLeases(TState state);
}
