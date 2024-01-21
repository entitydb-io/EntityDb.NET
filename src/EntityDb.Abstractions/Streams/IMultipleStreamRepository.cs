using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.States.Deltas;

namespace EntityDb.Abstractions.Streams;

/// <summary>
///     Encapsulates the source repository and manages
/// </summary>
public interface IMultipleStreamRepository : IDisposableResource
{
    /// <summary>
    ///     The backing source repository.
    /// </summary>
    ISourceRepository SourceRepository { get; }

    void Create(IStateKey streamKey);

    Task Load(IStateKey streamKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Load a stream if it exists, or create a new stream.
    /// </summary>
    /// <param name="streamKey">A key associated with the stream.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A task.</returns>
    Task LoadOrCreate(IStateKey streamKey, CancellationToken cancellationToken = default);

    void Append(IStateKey streamKey, object delta);
    
    /// <summary>
    ///     Stages a single delta with a given state key.
    /// </summary>
    /// <param name="streamKey">The key associated with the stream.</param>
    /// <param name="delta">The new delta that modifies the stream.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     Only <c>false</c> if <see cref="IAddMessageKeyDelta.GetMessageKey" />
    ///     returns a message key that already exists.
    /// </returns>
    Task<bool> Append<TDelta>(IStateKey streamKey, TDelta delta, CancellationToken cancellationToken = default)
        where TDelta : IAddMessageKeyDelta;

    /// <summary>
    ///     Commits all stages deltas.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>Only returns <c>false</c> if there are uncommitted messages.</returns>
    Task<bool> Commit(CancellationToken cancellationToken = default);
}
