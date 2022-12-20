using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

internal class EntitySnapshotTransactionCommandProcessor<TEntity> : ISnapshotTransactionCommandProcessor<TEntity>
{
    private readonly IEntityRepository<TEntity> _entityRepository;
    private readonly SnapshotTransactionCommandProcessorCache<TEntity> _snapshotTransactionCommandProcessorCache;

    public EntitySnapshotTransactionCommandProcessor
    (
        IEntityRepository<TEntity> entityRepository,
        SnapshotTransactionCommandProcessorCache<TEntity> snapshotTransactionCommandProcessorCache
    )
    {
        _entityRepository = entityRepository;
        _snapshotTransactionCommandProcessorCache = snapshotTransactionCommandProcessorCache;
    }

    public async Task<(TEntity?, TEntity)?> GetSnapshots(ITransaction transaction, ITransactionCommand transactionCommand, CancellationToken cancellationToken)
    {
        if (transactionCommand is not ITransactionCommandWithSnapshot transactioncommandWithSnapshot || transactioncommandWithSnapshot.Snapshot is not TEntity nextSnapshot)
        {
            return null;
        }

        var previousLatestPointer = transactionCommand.EntityId +
                                    transactionCommand.EntityVersionNumber.Previous();

        TEntity? previousLatestSnapshot = default;

        if (previousLatestPointer.VersionNumber != VersionNumber.MinValue)
        {
            previousLatestSnapshot = _snapshotTransactionCommandProcessorCache.GetSnapshotOrDefault(previousLatestPointer) ??
                                     await _entityRepository.GetSnapshot(previousLatestPointer, cancellationToken);
        }

        return (previousLatestSnapshot, nextSnapshot);
    }
}
