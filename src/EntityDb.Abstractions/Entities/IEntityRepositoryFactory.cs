using EntityDb.Abstractions.States;

namespace EntityDb.Abstractions.Entities;

/// <summary>
///     Represents a type used to create instances of <see cref="IMultipleEntityRepository{TEntity}" />
///     and <see cref="ISingleEntityRepository{TEntity}" />.
/// </summary>
/// <typeparam name="TEntity">The type of entity.</typeparam>
public interface IEntityRepositoryFactory<TEntity>
{
    /// <summary>
    ///     Create a new instance of <see cref="ISingleEntityRepository{TEntity}" />
    ///     for a new entity.
    /// </summary>
    /// <param name="entityId">A id associated with a <typeparamref name="TEntity" />.</param>
    /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
    /// <param name="sourceSessionOptionsName">The agent's use case for the source repository.</param>
    /// <param name="stateSessionOptionsName">The agent's use case for the state repository.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="ISingleEntityRepository{TEntity}" />.</returns>
    Task<ISingleEntityRepository<TEntity>> CreateSingleForNew
    (
        Id entityId,
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        string? stateSessionOptionsName = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Create a new instance of <see cref="ISingleEntityRepository{TEntity}" />
    ///     for an existing entity.
    /// </summary>
    /// <param name="entityPointer">A state pointer associated with a <typeparamref name="TEntity" />.</param>
    /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
    /// <param name="sourceSessionOptionsName">The agent's use case for the source repository.</param>
    /// <param name="stateSessionOptionsName">The agent's use case for the state repository.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="ISingleEntityRepository{TEntity}" />.</returns>
    Task<ISingleEntityRepository<TEntity>> CreateSingleForExisting
    (
        StatePointer entityPointer,
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        string? stateSessionOptionsName = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Create a new instance of <see cref="IMultipleEntityRepository{TEntity}" />
    /// </summary>
    /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
    /// <param name="sourceSessionOptionsName">The agent's use case for the source repository.</param>
    /// <param name="stateSessionOptionsName">The agent's use case for the state repository.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="IMultipleEntityRepository{TEntity}" />.</returns>
    Task<IMultipleEntityRepository<TEntity>> CreateMultiple
    (
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        string? stateSessionOptionsName = null,
        CancellationToken cancellationToken = default
    );
}
