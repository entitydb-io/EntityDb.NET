using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Snapshots;

/// <summary>
///     Represents a collection of <typeparamref name="TSnapshot" /> snapshots.
/// </summary>
/// <typeparam name="TSnapshot">The type of snapshot stored in the <see cref="ISnapshotRepository{TSnapshot}" />.</typeparam>
public interface ISnapshotRepository<TSnapshot> : IDisposableResource
{
    /// <ignore/>
    [Obsolete("Please use GetSnapshotOrDefault(...) instead. This method will be removed at a later date.")]
    [ExcludeFromCodeCoverage(Justification = "Obsolete")]
    public Task<TSnapshot?> GetSnapshot(Id snapshotId, CancellationToken cancellationToken = default)
        => GetSnapshotOrDefault(snapshotId, cancellationToken);

    /// <summary>
    ///     Returns an exact version of snapshot of a <typeparamref name="TSnapshot" /> or <c>default(<typeparamref name="TSnapshot"/>)</c>.
    /// </summary>
    /// <param name="snapshotPointer">A pointer to a specific snapshot.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An exact version of snapshot of a <typeparamref name="TSnapshot" /> or <c>default(<typeparamref name="TSnapshot"/>)</c>.</returns>
    Task<TSnapshot?> GetSnapshotOrDefault(Pointer snapshotPointer, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Inserts a <typeparamref name="TSnapshot" /> snapshot.
    /// </summary>
    /// <param name="snapshotPointer">A pointer to a specific snapshot.</param>
    /// <param name="snapshot">The snapshot.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the insert succeeded, or <c>false</c> if the insert failed.</returns>
    Task<bool> PutSnapshot(Pointer snapshotPointer, TSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes multiple <typeparamref name="TSnapshot" /> snapshots.
    /// </summary>
    /// <param name="snapshotPointers">Pointers to specific snapshots.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the deletes all succeeded, or <c>false</c> if any deletes failed.</returns>
    Task<bool> DeleteSnapshots(Pointer[] snapshotPointers, CancellationToken cancellationToken = default);
}
