using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.States.Deltas;

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
    ///     The state pointer for the current stream.
    /// </summary>
    IStateKey StreamKey { get; }

    void Append(object delta);
    
    /// <summary>
    ///     Stages a single delta with a given state key and message key.
    /// </summary>
    /// <param name="delta">The new delta that modifies the stream.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     Only async if the delta implements <see cref="IAddMessageKeyDelta" />.
    ///     Only <c>false</c> if the the delta implements <see cref="IAddMessageKeyDelta" /> and
    ///     the message key already exists.
    /// </returns>
    Task<bool> Append<TDelta>(TDelta delta, CancellationToken cancellationToken = default)
        where TDelta : IAddMessageKeyDelta;

    /// <summary>
    ///     Commits all stages deltas.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>Only returns <c>false</c> if there are uncommitted messages.</returns>
    Task<bool> Commit(CancellationToken cancellationToken = default);
}
