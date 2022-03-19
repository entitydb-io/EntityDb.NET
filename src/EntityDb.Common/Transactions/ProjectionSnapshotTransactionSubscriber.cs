using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Annotations;
using EntityDb.Common.Projections;
using EntityDb.Common.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions;

internal class ProjectionSnapshotTransactionSubscriber<TProjection> : TransactionSubscriber
    where TProjection : IProjection<TProjection>, ISnapshot<TProjection>
{
    private readonly IProjectionStrategy<TProjection> _projectionStrategy;
    private readonly ISnapshotRepositoryFactory<TProjection> _snapshotRepositoryFactory;
    private readonly string _snapshotSessionOptionsName;

    public ProjectionSnapshotTransactionSubscriber
    (
        IProjectionStrategy<TProjection> projectionStrategy,
        ISnapshotRepositoryFactory<TProjection> snapshotRepositoryFactory,
        string snapshotSessionOptionsName,
        bool synchronousMode
    ) : base(synchronousMode)
    {
        _projectionStrategy = projectionStrategy;
        _snapshotRepositoryFactory = snapshotRepositoryFactory;
        _snapshotSessionOptionsName = snapshotSessionOptionsName;
    }

    protected override async Task NotifyAsync(ITransaction transaction)
    {
        var snapshotRepository =
            await _snapshotRepositoryFactory.CreateRepository(_snapshotSessionOptionsName);

        var steps = transaction.Steps
            .Where(step => step is IAppendCommandTransactionStep)
            .Cast<IAppendCommandTransactionStep>();

        var projectionCache = new Dictionary<Id, TProjection>();
        
        foreach (var stepGroup in steps)
        {
            var projectionIds = await _projectionStrategy.GetProjectionIds(stepGroup.EntityId);
            
            foreach (var projectionId in projectionIds)
            {
                projectionCache.TryGetValue(projectionId, out var projection);
                
                projection ??= await snapshotRepository.GetSnapshot(projectionId) ?? TProjection.Construct(projectionId);

                var previousProjection = projection;
                
                var annotatedCommand = EntityAnnotation<object>.CreateFrom(transaction, stepGroup, stepGroup.Command);

                projection = projection.Reduce(annotatedCommand);

                if (projection.ShouldReplace(previousProjection))
                {
                    await snapshotRepository.PutSnapshot(projectionId, projection);
                }

                projectionCache[projectionId] = projection;
            }
        }
    }

    public static ProjectionSnapshotTransactionSubscriber<TProjection> Create(IServiceProvider serviceProvider,
        string snapshotSessionOptionsName, bool synchronousMode)
    {
        return ActivatorUtilities.CreateInstance<ProjectionSnapshotTransactionSubscriber<TProjection>>(serviceProvider,
            snapshotSessionOptionsName,
            synchronousMode);
    }
}
