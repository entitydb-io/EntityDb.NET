using EntityDb.Abstractions.Leases;

namespace EntityDb.Abstractions.Commands;

/// <summary>
///     Represents a command that adds leases.
/// </summary>
public interface IAddLeasesCommand
{
    /// <summary>
    ///     Returns the leases that need to be added.
    /// </summary>
    /// <returns>The leases that need to be added.</returns>
    IEnumerable<ILease> GetLeases();
}
