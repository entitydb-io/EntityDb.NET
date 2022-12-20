using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Annotations;
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
        if (transactionCommand is not ITransactionCommandWithSnapshot transactionCommandWithSnapshot)
        {
            return null;
        }

        var projectionId = _projectionRepository.GetProjectionIdOrDefault(transactionCommandWithSnapshot.Snapshot);

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

        var annotatedCommand = EntityAnnotation<object>.CreateFromBoxedData
        (
            transaction.Id,
            transaction.TimeStamp,
            transactionCommand.EntityId,
            transactionCommand.EntityVersionNumber,
            transactionCommand.Command
        );

        var nextSnapshot =
            (previousLatestSnapshot ?? TProjection.Construct(projectionId.Value)).Reduce(annotatedCommand);

        return (previousLatestSnapshot, nextSnapshot);
    }
}
