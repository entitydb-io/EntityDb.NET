using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.ValueObjects;

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
    IEnumerable<ILease> GetLeases(Id entityId, VersionNumber entityVersionNumber);

    /// <ignore />
    [Obsolete("Please us GetLeases(Id, VersionNumber) instead. This will be removed in a future version.", true)]
    IEnumerable<ILease> GetLeases() => throw new NotImplementedException();
}
