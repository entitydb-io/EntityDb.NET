using Microsoft.EntityFrameworkCore;

namespace EntityDb.EntityFramework.Snapshots;

/// <summary>
///     The base DbContext
/// </summary>
/// <typeparam name="TSnapshot">The type of the snapshot</typeparam>
public interface ISnapshotDbContext<TSnapshot>
    where TSnapshot : class
{
    /// <summary>
    ///     A database set for resolving specific snapshots from pointers.
    /// </summary>
    DbSet<SnapshotReference<TSnapshot>> SnapshotReferences { get; set; }

    /// <summary>
    ///     A database set for the root snapshots.
    /// </summary>
    DbSet<TSnapshot> Snapshots { get; set; }
}
