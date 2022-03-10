using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using System;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Projections;

/// <summary>
///     Encapsulates the snapshot repository for a projection.
/// </summary>
/// <typeparam name="TProjection">The type of the projection.</typeparam>
public interface IProjectionRepository<TProjection> : IDisposableResource
{
    //TODO: Getter for the projection strategy here

    ITransactionRepository TransactionRepository { get; }
    
    /// <summary>
    ///     The backing snapshot repository.
    /// </summary>
    ISnapshotRepository<TProjection> SnapshotRepository { get; }

    /// <summary>
    ///     Returns the current state of a <typeparamref name="TProjection" />.
    /// </summary>
    /// <param name="projectionId">The id of the projection.</param>
    /// <returns>The current state of a <typeparamref name="TProjection" />.</returns>
    Task<TProjection> GetCurrent(Id projectionId);
}
