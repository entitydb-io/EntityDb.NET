using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Common.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions;

internal class EntitySnapshotTransactionSubscriber<TEntity> : SnapshotTransactionSubscriberBase<TEntity>
    where TEntity : IEntity<TEntity>
{    
    public EntitySnapshotTransactionSubscriber
    (
        ISnapshotRepositoryFactory<TEntity> snapshotRepositoryFactory,
        string snapshotSessionOptionsName,
        bool testMode
    ) : base(snapshotRepositoryFactory, snapshotSessionOptionsName, testMode)
    {
    }

    protected override async Task<(TEntity? previousMostRecentSnapshot, TEntity nextSnapshot)?> GetSnapshots(ITransaction transaction, ITransactionStep transactionStep, ISnapshotRepository<TEntity> snapshotRepository)
    {
        if (transactionStep.Entity is not TEntity nextSnapshot)
        {
            return null;
        }

        var previousMostRecentSnapshot = await snapshotRepository.GetSnapshot(transactionStep.EntityId);

        return (previousMostRecentSnapshot, nextSnapshot);
    }

    public static EntitySnapshotTransactionSubscriber<TEntity> Create(IServiceProvider serviceProvider,
        string snapshotSessionOptionsName, bool testMode)
    {
        return ActivatorUtilities.CreateInstance<EntitySnapshotTransactionSubscriber<TEntity>>(serviceProvider,
            snapshotSessionOptionsName,
            testMode);
    }
}
