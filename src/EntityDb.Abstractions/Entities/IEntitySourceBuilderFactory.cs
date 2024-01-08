using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Entities;

/// <summary>
///     Represents a type used to create instances of <see cref="IEntitySourceBuilder{TEntity}" /> or
///     <see cref="ISingleEntitySourceBuilder{TEntity}" />.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IEntitySourceBuilderFactory<TEntity>
{
    /// <summary>
    ///     Creates a new instance of <see cref="IEntitySourceBuilder{TEntity}" />.
    /// </summary>
    /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="IEntitySourceBuilder{TEntity}" />.</returns>
    Task<IEntitySourceBuilder<TEntity>> Create(string agentSignatureOptionsName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new instance of <see cref="ISingleEntitySourceBuilder{TEntity}" />.
    /// </summary>
    /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
    /// <param name="entityId">The id of the entity.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="ISingleEntitySourceBuilder{TEntity}" />.</returns>
    Task<ISingleEntitySourceBuilder<TEntity>> CreateForSingleEntity(string agentSignatureOptionsName,
        Id entityId,
        CancellationToken cancellationToken = default);
}
