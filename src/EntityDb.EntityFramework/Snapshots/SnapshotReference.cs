using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.EntityFramework.Snapshots;

/// <summary>
///     Represents a unique snapshot and its pointer.
/// </summary>
/// <typeparam name="TSnapshot"></typeparam>
public sealed class SnapshotReference<TSnapshot>
{
    /// <summary>
    ///     Te ID of the Reference record.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    ///     The ID of the Snapshot Pointer.
    /// </summary>
    public required Id PointerId { get; init; }

    /// <summary>
    ///     The Version Number of the Snapshot Pointer.
    /// </summary>
    public required VersionNumber PointerVersionNumber { get; init; }

    /// <summary>
    ///     The Id of the Snapshot.
    /// </summary>
    public required Id SnapshotId { get; set; }

    /// <summary>
    ///     The VersionNumber of the Snapshot.
    /// </summary>
    public required VersionNumber SnapshotVersionNumber { get; set; }

    /// <summary>
    ///     The Snapshot.
    /// </summary>
    public required TSnapshot Snapshot { get; set; }
}
