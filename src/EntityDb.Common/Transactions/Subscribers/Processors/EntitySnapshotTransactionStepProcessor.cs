using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

internal class EntitySnapshotTransactionStepProcessor<TEntity> : ISnapshotTransactionStepProcessor<TEntity>
{
    private readonly IEntityRepository<TEntity> _entityRepository;
    private readonly SnapshotTransactionStepProcessorCache<TEntity> _snapshotTransactionStepProcessorCache;

    public EntitySnapshotTransactionStepProcessor
    (
        IEntityRepository<TEntity> entityRepository,
        SnapshotTransactionStepProcessorCache<TEntity> snapshotTransactionStepProcessorCache
    )
    {
        _entityRepository = entityRepository;
        _snapshotTransactionStepProcessorCache = snapshotTransactionStepProcessorCache;
    }

    public async Task<(TEntity?, TEntity)?> GetSnapshots(ITransaction transaction, ITransactionStep transactionStep, CancellationToken cancellationToken)
    {
        if (transactionStep is not IAppendCommandTransactionStep appendCommandTransactionStep || appendCommandTransactionStep.Entity is not TEntity nextSnapshot)
        {
            return null;
        }

        var previousLatestPointer = appendCommandTransactionStep.EntityId +
                                    appendCommandTransactionStep.PreviousEntityVersionNumber;

        TEntity? previousLatestSnapshot = default;

        if (previousLatestPointer.VersionNumber != VersionNumber.MinValue)
        {
            previousLatestSnapshot = _snapshotTransactionStepProcessorCache.GetSnapshotOrDefault(previousLatestPointer) ??
                                     await _entityRepository.GetSnapshot(previousLatestPointer, cancellationToken);
        }

        return (previousLatestSnapshot, nextSnapshot);
    }
}
