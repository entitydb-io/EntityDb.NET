using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Projections;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

internal sealed class ProjectionSnapshotTransactionCommandProcessor<TProjection> : ISnapshotTransactionCommandProcessor<TProjection>
    where TProjection : IProjection<TProjection>
{
    private readonly IProjectionRepository<TProjection> _projectionRepository;
    private readonly SnapshotTransactionCommandProcessorCache<TProjection> _snapshotTransactionCommandProcessorCache;

    public ProjectionSnapshotTransactionCommandProcessor
    (
        IProjectionRepository<TProjection> projectionRepository,
        SnapshotTransactionCommandProcessorCache<TProjection> snapshotTransactionCommandProcessorCache
    )
    {
        _projectionRepository = projectionRepository;
        _snapshotTransactionCommandProcessorCache = snapshotTransactionCommandProcessorCache;
    }

    public async Task<(TProjection?, TProjection)?> GetSnapshots
    (
        ITransaction transaction,
        ITransactionCommand transactionCommand,
        CancellationToken cancellationToken
    )
    {
        var projectionId = _projectionRepository.GetProjectionIdOrDefault(transaction, transactionCommand);

        if (projectionId is null)
        {
            return null;
        }

        var previousLatestPointer = projectionId.Value + transactionCommand.EntityVersionNumber.Previous();

        TProjection? previousLatestSnapshot = default;

        if (previousLatestPointer.VersionNumber != VersionNumber.MinValue)
        {
            previousLatestSnapshot = _snapshotTransactionCommandProcessorCache.GetSnapshotOrDefault(previousLatestPointer) ??
                                     await _projectionRepository.GetSnapshot(previousLatestPointer,
                                         cancellationToken);
        }

        var nextSnapshot =
            (previousLatestSnapshot ?? TProjection.Construct(projectionId.Value)).Reduce(transaction, transactionCommand);

        return (previousLatestSnapshot, nextSnapshot);
    }
}
