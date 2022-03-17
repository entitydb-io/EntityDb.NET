using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Snapshots;

/// <summary>
///     Represents a collection of <typeparamref name="TSnapshot" /> snapshots.
/// </summary>
/// <typeparam name="TSnapshot">The type of snapshot stored in the <see cref="ISnapshotRepository{TSnapshot}" />.</typeparam>
public interface ISnapshotRepository<TSnapshot> : IDisposableResource
{
    /// <summary>
    ///     Returns a <typeparamref name="TSnapshot" /> snapshot or <c>default(<typeparamref name="TSnapshot" />)</c>.
    /// </summary>
    /// <param name="snapshotId">The id of the snapshot.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <typeparamref name="TSnapshot" /> snapshot or <c>default(<typeparamref name="TSnapshot" />)</c>.</returns>
    Task<TSnapshot?> GetSnapshot(Id snapshotId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Inserts a <typeparamref name="TSnapshot" /> snapshot.
    /// </summary>
    /// <param name="snapshotId">The id of the snapshot.</param>
    /// <param name="snapshot">The snapshot.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the insert succeeded, or <c>false</c> if the insert failed.</returns>
    Task<bool> PutSnapshot(Id snapshotId, TSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes multiple <typeparamref name="TSnapshot" /> snapshots.
    /// </summary>
    /// <param name="snapshotIds">The ids of the snapshots to delete.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the deletes all succeeded, or <c>false</c> if any deletes failed.</returns>
    Task<bool> DeleteSnapshots(Id[] snapshotIds, CancellationToken cancellationToken = default);
}
