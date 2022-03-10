using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions;

internal class EntitySnapshotTransactionSubscriber<TSnapshot> : TransactionSubscriber
    where TSnapshot : ISnapshot<TSnapshot>
{
    private readonly ISnapshotRepositoryFactory<TSnapshot> _snapshotRepositoryFactory;
    private readonly string _snapshotSessionOptionsName;

    public EntitySnapshotTransactionSubscriber
    (
        ISnapshotRepositoryFactory<TSnapshot> snapshotRepositoryFactory,
        string snapshotSessionOptionsName,
        bool synchronousMode
    ) : base(synchronousMode)
    {
        _snapshotRepositoryFactory = snapshotRepositoryFactory;
        _snapshotSessionOptionsName = snapshotSessionOptionsName;
    }

    protected override async Task NotifyAsync(ITransaction transaction)
    {
        var snapshotRepository =
            await _snapshotRepositoryFactory.CreateRepository(_snapshotSessionOptionsName);

        var stepGroups = transaction.Steps
            .GroupBy(step => step.EntityId);

        foreach (var stepGroup in stepGroups)
        {
            var entity = stepGroup.Last().Entity;

            if (entity is not TSnapshot nextSnapshot)
            {
                return;
            }
            
            var snapshotId = stepGroup.Key;
            
            var previousSnapshot = await snapshotRepository.GetSnapshot(snapshotId);

            if (!nextSnapshot.ShouldReplace(previousSnapshot))
            {
                continue;
            }

            await snapshotRepository.PutSnapshot(snapshotId, nextSnapshot);
        }
    }

    public static EntitySnapshotTransactionSubscriber<TSnapshot> Create(IServiceProvider serviceProvider,
        string snapshotSessionOptionsName, bool synchronousMode)
    {
        return ActivatorUtilities.CreateInstance<EntitySnapshotTransactionSubscriber<TSnapshot>>(serviceProvider,
            snapshotSessionOptionsName,
            synchronousMode);
    }
}
