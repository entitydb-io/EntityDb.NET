using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions.Processors;

internal class EntitySnapshotTransactionProcessor<TEntity> : SnapshotTransactionProcessorBase<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IEntityRepositoryFactory<TEntity> _entityRepositoryFactory;
    private readonly string _snapshotSessionOptionsName;
    private readonly string _transactionSessionOptionsName;

    public EntitySnapshotTransactionProcessor
    (
        IEntityRepositoryFactory<TEntity> entityRepositoryFactory,
        string transactionSessionOptionsName,
        string snapshotSessionOptionsName
    )
    {
        _entityRepositoryFactory = entityRepositoryFactory;
        _transactionSessionOptionsName = transactionSessionOptionsName;
        _snapshotSessionOptionsName = snapshotSessionOptionsName;
    }

    public override async Task ProcessTransaction(ITransaction transaction, CancellationToken cancellationToken)
    {
        await using var entityRepository = await _entityRepositoryFactory.CreateRepository(
            _transactionSessionOptionsName, _snapshotSessionOptionsName, cancellationToken);

        if (entityRepository.SnapshotRepository is null)
        {
            return;
        }

        var snapshotCache = CreateSnapshotCache();

        async Task<(TEntity?, TEntity)?> GetSnapshots(IAppendCommandTransactionStep appendCommandTransactionStep)
        {
            if (appendCommandTransactionStep.Entity is not TEntity nextSnapshot)
            {
                return null;
            }

            var previousLatestPointer = appendCommandTransactionStep.EntityId +
                                        appendCommandTransactionStep.PreviousEntityVersionNumber;

            TEntity? previousLatestSnapshot = default;

            if (previousLatestPointer.VersionNumber != VersionNumber.MinValue)
            {
                previousLatestSnapshot = snapshotCache.GetSnapshotOrDefault(previousLatestPointer) ??
                                         await entityRepository.GetSnapshot(previousLatestPointer, cancellationToken);
            }

            return (previousLatestSnapshot, nextSnapshot);
        }

        await ProcessTransactionSteps(entityRepository.SnapshotRepository, snapshotCache, transaction, GetSnapshots,
            cancellationToken);
    }

    public static EntitySnapshotTransactionProcessor<TEntity> Create(IServiceProvider serviceProvider,
        string transactionSessionOptionsName, string snapshotSessionOptionsName)
    {
        return ActivatorUtilities.CreateInstance<EntitySnapshotTransactionProcessor<TEntity>>(serviceProvider,
            transactionSessionOptionsName, snapshotSessionOptionsName);
    }
}
