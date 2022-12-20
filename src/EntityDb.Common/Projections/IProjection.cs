using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Projections;

/// <summary>
///     Provides basic functionality for the common implementation of projections.
/// </summary>
/// <typeparam name="TProjection"></typeparam>
public interface IProjection<TProjection> : ISnapshot<TProjection>
{
    /// <summary>
    ///     Returns a new <typeparamref name="TProjection" /> that incorporates the transaction command.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="transactionCommand"></param>
    /// <returns></returns>
    TProjection Reduce(ITransaction transaction, ITransactionCommand transactionCommand);

    /// <summary>
    ///     Returns a <see cref="ICommandQuery" /> that finds transaction commands that need to be passed to the reducer.
    /// </summary>
    /// <param name="projectionPointer">A pointer to the desired projection state</param>
    /// <param name="transactionRepository">The transaction repository, which can be used to locate new information</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A <see cref="ICommandQuery" /> that is used to load the rest of the transaction commands for the given projection pointer.</returns>
    /// <remarks>
    ///     I would only recommend using the transaction repository to locate leases or tags, not commands or agent signatures.
    /// </remarks>
    Task<ICommandQuery> GetReducersQuery(Pointer projectionPointer, ITransactionRepository transactionRepository, CancellationToken cancellationToken);

    /// <summary>
    ///     Maps an entity to a projection id, or default if the entity does not map to this projection.
    /// </summary>
    /// <param name="transaction">The transaction that could trigger a projection</param>
    /// <param name="transactionCommand">The transaction command that could trigger a projection</param>
    /// <returns>The projection id for the entity, or default if none.</returns>
    static abstract Id? GetProjectionIdOrDefault(ITransaction transaction, ITransactionCommand transactionCommand);
}
