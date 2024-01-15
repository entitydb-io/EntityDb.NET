using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.States;

namespace EntityDb.Abstractions.Projections;

/// <summary>
///     Encapsulates the state repository for a projection.
/// </summary>
/// <typeparam name="TProjection">The type of the projection.</typeparam>
public interface IProjectionRepository<TProjection> : IDisposableResource
{
    /// <summary>
    ///     The backing state repository.
    /// </summary>
    IStateRepository<TProjection>? StateRepository { get; }

    /// <summary>
    ///     Returns the state of a <typeparamref name="TProjection" /> for a given <see cref="StatePointer" />.
    /// </summary>
    /// <param name="projectionPointer">The state pointer to the projection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The state of a <typeparamref name="TProjection" /> for <paramref name="projectionPointer" />.</returns>
    Task<TProjection> Get(StatePointer projectionPointer, CancellationToken cancellationToken = default);
}
