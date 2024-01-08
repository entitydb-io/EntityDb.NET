using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Snapshots;

/// <summary>
///     Indicates that the snapshot is compatible with several EntityDb.Common implementations.
/// </summary>
/// <typeparam name="TSnapshot">The type of the snapshot.</typeparam>
public interface ISnapshot<TSnapshot>
{
    /// <summary>
    ///     Creates a new instance of a <typeparamref name="TSnapshot" />.
    /// </summary>
    /// <param name="pointer">The pointer of the snapshot.</param>
    /// <returns>A new instance of <typeparamref name="TSnapshot" />.</returns>
    static abstract TSnapshot Construct(Pointer pointer);

    /// <summary>
    ///     Returns a pointer for the current state of the snapshot.
    /// </summary>
    /// <returns>A pointer for the current state of the snapshot</returns>
    Pointer GetPointer();

    /// <summary>
    ///     Indicates if this snapshot instance version should be recorded (independent of the latest snapshot).
    /// </summary>
    /// <returns><c>true</c> if this snapshot instance should be recorded, or else <c>false</c>.</returns>
    /// <remarks>
    ///     You would use this if you intent to fetch a snapshot at multiple versions and don't want to hit
    ///     the source database when it can be avoided.
    /// </remarks>
    bool ShouldRecord();

    /// <summary>
    ///     Indicates if this snapshot instance should be recorded as the latest snapshot.
    /// </summary>
    /// <param name="previousLatestSnapshot">The previous instance of the latest snapshot.</param>
    /// <returns><c>true</c> if this snapshot instance should be recorded as the latest snapshot, or else <c>false</c>.</returns>
    bool ShouldRecordAsLatest(TSnapshot? previousLatestSnapshot);
}
