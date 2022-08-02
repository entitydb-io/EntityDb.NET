namespace EntityDb.Abstractions.Entities;

/// <summary>
///     Represents a type used to create instances of <see cref="IEntityRepository{TEntity}" />
/// </summary>
/// <typeparam name="TEntity">The type of entity managed by the <see cref="IEntityRepository{TEntity}" />.</typeparam>
public interface IEntityRepositoryFactory<TEntity>
{
    /// <summary>
    ///     Create a new instance of <see cref="IEntityRepository{TEntity}" />
    /// </summary>
    /// <param name="transactionSessionOptionsName">The agent's use case for the transaction repository.</param>
    /// <param name="snapshotSessionOptionsName">The agent's use case for the snapshot repository.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="IEntityRepository{TEntity}" />.</returns>
    Task<IEntityRepository<TEntity>> CreateRepository(string transactionSessionOptionsName,
        string? snapshotSessionOptionsName = null, CancellationToken cancellationToken = default);
}
