using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions;

internal class EntitySnapshotTransactionSubscriber<TEntity> : TransactionSubscriber
    where TEntity : IEntity<TEntity>, ISnapshot<TEntity>
{
    private readonly ISnapshotRepositoryFactory<TEntity> _snapshotRepositoryFactory;
    private readonly string _snapshotSessionOptionsName;
    
    public EntitySnapshotTransactionSubscriber
    (
        ISnapshotRepositoryFactory<TEntity> snapshotRepositoryFactory,
        string snapshotSessionOptionsName,
        bool testMode
    ) : base(testMode)
    {
        _snapshotRepositoryFactory = snapshotRepositoryFactory;
        _snapshotSessionOptionsName = snapshotSessionOptionsName;
    }

    public Task<ISnapshotRepository<TEntity>> CreateSnapshotRepository()
    {
        return _snapshotRepositoryFactory.CreateRepository(_snapshotSessionOptionsName);
    }

    protected override async Task NotifyAsync(ITransaction transaction)
    {
        await using var snapshotRepository = new BulkOptimizedSnapshotRepository<TEntity>(await CreateSnapshotRepository());

        foreach (var step in transaction.Steps)
        {
            if (step.Entity is not TEntity nextSnapshot)
            {
                continue;
            }

            var entityId = nextSnapshot.GetId();

            var previousSnapshot = await snapshotRepository.GetSnapshot(entityId);

            if (nextSnapshot.ShouldReplace(previousSnapshot))
            {
                await snapshotRepository.PutSnapshot(entityId, nextSnapshot);
            }
        }

        await snapshotRepository.PutSnapshots();
    }

    public static EntitySnapshotTransactionSubscriber<TEntity> Create(IServiceProvider serviceProvider,
        string snapshotSessionOptionsName, bool testMode)
    {
        return ActivatorUtilities.CreateInstance<EntitySnapshotTransactionSubscriber<TEntity>>(serviceProvider,
            snapshotSessionOptionsName,
            testMode);
    }
}
