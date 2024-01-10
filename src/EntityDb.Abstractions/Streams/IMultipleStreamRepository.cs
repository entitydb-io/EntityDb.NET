﻿using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;

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

    /// <summary>
    ///     Load a stream if it exists, or create a new stream.
    /// </summary>
    /// <param name="streamKey">A key associated with the stream.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A task.</returns>
    Task LoadOrCreate(Key streamKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Stages a single delta with a given state key and message key.
    /// </summary>
    /// <param name="streamKey">The key associated with the stream.</param>
    /// <param name="messageKey">The key associated with the message.</param>
    /// <param name="delta">The new delta that modifies the stream.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Only <c>false</c> if the message key already exists.</returns>
    Task<bool> Stage(Key streamKey, Key messageKey, object delta, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Commits all stages deltas.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns><c>true</c> if the commit succeeded, or <c>false</c> if the commit failed.</returns>
    Task<bool> Commit(CancellationToken cancellationToken = default);
}
