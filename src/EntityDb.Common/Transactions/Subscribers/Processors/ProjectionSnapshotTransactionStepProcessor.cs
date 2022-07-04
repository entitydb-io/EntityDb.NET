using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Annotations;
using EntityDb.Common.Projections;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

internal sealed class ProjectionSnapshotTransactionStepProcessor<TProjection> : ISnapshotTransactionStepProcessor<TProjection>
    where TProjection : IProjection<TProjection>
{
    private readonly IProjectionRepository<TProjection> _projectionRepository;
    private readonly SnapshotTransactionStepProcessorCache<TProjection> _snapshotTransactionStepProcessorCache;

    public ProjectionSnapshotTransactionStepProcessor
    (
        IProjectionRepository<TProjection> projectionRepository,
        SnapshotTransactionStepProcessorCache<TProjection> snapshotTransactionStepProcessorCache
    )
    {
        _projectionRepository = projectionRepository;
        _snapshotTransactionStepProcessorCache = snapshotTransactionStepProcessorCache;
    }

    public async Task<(TProjection?, TProjection)?> GetSnapshots
    (
        ITransaction transaction,
        ITransactionStep transactionStep,
        CancellationToken cancellationToken
    )
    {
        if (transactionStep is not IAppendCommandTransactionStep appendCommandTransactionStep)
        {
            return null;
        }

        var projectionId = _projectionRepository.GetProjectionIdOrDefault(appendCommandTransactionStep.Entity);

        if (projectionId is null)
        {
            return null;
        }

        var previousLatestPointer = projectionId.Value + appendCommandTransactionStep.PreviousEntityVersionNumber;

        TProjection? previousLatestSnapshot = default;

        if (previousLatestPointer.VersionNumber != VersionNumber.MinValue)
        {
            previousLatestSnapshot = _snapshotTransactionStepProcessorCache.GetSnapshotOrDefault(previousLatestPointer) ??
                                     await _projectionRepository.GetSnapshot(previousLatestPointer,
                                         cancellationToken);
        }

        var annotatedCommand = EntityAnnotation<object>.CreateFromBoxedData
        (
            transaction.Id,
            transaction.TimeStamp,
            appendCommandTransactionStep.EntityId,
            appendCommandTransactionStep.EntityVersionNumber,
            appendCommandTransactionStep.Command
        );

        var nextSnapshot =
            (previousLatestSnapshot ?? TProjection.Construct(projectionId.Value)).Reduce(annotatedCommand);

        return (previousLatestSnapshot, nextSnapshot);
    }
}
