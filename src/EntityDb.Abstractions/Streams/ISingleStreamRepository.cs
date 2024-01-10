using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Streams;

/// <summary>
///     Encapsulates the source repository and manages
/// </summary>
public interface ISingleStreamRepository : IDisposableResource
{
    /// <summary>
    ///     The backing source repository.
    /// </summary>
    ISourceRepository SourceRepository { get; }

    /// <summary>
    ///     The pointer for the current stream.
    /// </summary>
    Key StreamKey { get; }

    /// <summary>
    ///     Stages a single delta with a given state key and message key.
    /// </summary>
    /// <param name="messageKey">The key associated with the message.</param>
    /// <param name="delta">The new delta that modifies the stream.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Only <c>false</c> if the message key already exists.</returns>
    Task<bool> Stage(Key messageKey, object delta, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Commits all stages deltas.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns><c>true</c> if the commit succeeded, or <c>false</c> if the commit failed.</returns>
    Task<bool> Commit(CancellationToken cancellationToken = default);
}
