using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.EventStreams;

/// <summary>
///     Encapsulates the source repository and manages
/// </summary>
public interface IEventStreamRepository : IDisposableResource
{
    /// <summary>
    ///     The backing source repository.
    /// </summary>
    ISourceRepository SourceRepository { get; }

    /// <summary>
    ///     Stages a single delta with a given stream id and event id.
    /// </summary>
    /// <param name="streamKey">The key associated with the stream.</param>
    /// <param name="eventKey">A key associated with the event.</param>
    /// <param name="delta">The new delta that modifies the event stream.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Only <c>false</c> if the event key already exists.</returns>
    /// <remarks>
    ///     If a duplicate event key is received for a given stream key, the delta
    ///     will be skipped.
    /// </remarks>
    Task<bool> Stage(Key streamKey, Key eventKey, object delta, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Commits all stages deltas.
    /// </summary>
    /// <param name="sourceId">A new id for the new source.</param>
    /// <param name="maxAttempts">How many attempts should be made to commit the source?</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns><c>true</c> if the commit succeeded, or <c>false</c> if the commit failed.</returns>
    Task<bool> Commit(Id sourceId, byte maxAttempts = 3, CancellationToken cancellationToken = default);
}
