namespace EntityDb.Abstractions.Projections;

/// <summary>
///     Represents a type used to create instances of <see cref="IProjectionRepository{TProjection}" />
/// </summary>
/// <typeparam name="TProjection">The type of projection managed by the <see cref="IProjectionRepository{TProjection}" />.</typeparam>
public interface IProjectionRepositoryFactory<TProjection>
{
    /// <summary>
    ///     Create a new instance of <see cref="IProjectionRepository{TProjection}" />
    /// </summary>
    /// <param name="transactionSessionOptionsName">The agent's use case for the transaction repository.</param>
    /// <param name="snapshotSessionOptionsName">The agent's use case for the snapshot repository.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="IProjectionRepository{TProjection}" />.</returns>
    Task<IProjectionRepository<TProjection>> CreateRepository(string transactionSessionOptionsName,
        string snapshotSessionOptionsName, CancellationToken cancellationToken = default);
}
