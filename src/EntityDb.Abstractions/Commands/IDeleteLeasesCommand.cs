using EntityDb.Abstractions.Leases;

namespace EntityDb.Abstractions.Commands;

/// <summary>
///     Represents a command that deletes leases.
/// </summary>
public interface IDeleteLeasesCommand
{
    /// <summary>
    ///     Returns the leases that need to be deleted.
    /// </summary>
    /// <returns>The leases that need to be deleted.</returns>
    IEnumerable<ILease> GetLeases();
}
