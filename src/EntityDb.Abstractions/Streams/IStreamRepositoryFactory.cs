using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Abstractions.Streams;

/// <summary>
///     Represents a type used to create instances of <see cref="IMultipleStreamRepository" />.
/// </summary>
public interface IStreamRepositoryFactory
{
    /// <summary>
    ///     Create a new instance of <see cref="ISingleEntityRepository{TEntity}" />
    ///     for an existing stream.
    /// </summary>
    /// <param name="streamKey">A key associated with a stream.</param>
    /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
    /// <param name="sourceSessionOptionsName">The agent's use case for the source repository.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="ISingleEntityRepository{TEntity}" />.</returns>
    Task<ISingleStreamRepository> CreateSingle
    (
        IStateKey streamKey,
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        CancellationToken cancellationToken = default
    );

    Task<ISingleStreamRepository> CreateSingleForNew
    (
        IStateKey streamKey,
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        CancellationToken cancellationToken = default
    );

    Task<ISingleStreamRepository> CreateSingleForExisting
    (
        IStateKey streamKey,
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Creates a new instance of <see cref="IMultipleStreamRepository" />.
    /// </summary>
    /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
    /// <param name="sourceSessionOptionsName">The name of the source session options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="IMultipleStreamRepository" />.</returns>
    Task<IMultipleStreamRepository> CreateMultiple(string agentSignatureOptionsName,
        string sourceSessionOptionsName, CancellationToken cancellationToken = default);
}
