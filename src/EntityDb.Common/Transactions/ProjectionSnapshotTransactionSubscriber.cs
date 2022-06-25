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
        bool testMode
    ) : base(testMode)
    {
        _projectionStrategy = projectionStrategy;
        _snapshotRepositoryFactory = snapshotRepositoryFactory;
        _snapshotSessionOptionsName = snapshotSessionOptionsName;
    }

    public Task<ISnapshotRepository<TProjection>> CreateSnapshotRepository()
    {
        return _snapshotRepositoryFactory.CreateRepository(_snapshotSessionOptionsName);
    }

    protected override async Task NotifyAsync(ITransaction transaction)
    {
        await using var snapshotRepository = await CreateSnapshotRepository();

        var projectionCache = new Dictionary<Id, TProjection>();
        
        foreach (var step in transaction.Steps)
        {
            if (step is not IAppendCommandTransactionStep appendCommandTransactionStep)
            {
                continue;
            }
            
            var projectionId = await _projectionStrategy.GetProjectionId(appendCommandTransactionStep.EntityId, appendCommandTransactionStep.Entity);
            
            projectionCache.TryGetValue(projectionId, out var previousProjection);
                
            previousProjection ??= await snapshotRepository.GetSnapshot(projectionId) ?? TProjection.Construct(projectionId);

            var projection = previousProjection;
                
            var annotatedCommand = EntityAnnotation<object>.CreateFromBoxedData
            (
                transaction.Id,
                transaction.TimeStamp,
                appendCommandTransactionStep.EntityId,
                appendCommandTransactionStep.EntityVersionNumber,
                appendCommandTransactionStep.Command
            );

            projection = projection.Reduce(annotatedCommand);

            if (projection.ShouldReplace(previousProjection))
            {
                await snapshotRepository.PutSnapshot(projectionId, projection);
            }

            projectionCache[projectionId] = projection;
        }
    }

    public static ProjectionSnapshotTransactionSubscriber<TProjection> Create(IServiceProvider serviceProvider,
        string snapshotSessionOptionsName, bool testMode)
    {
        return ActivatorUtilities.CreateInstance<ProjectionSnapshotTransactionSubscriber<TProjection>>(serviceProvider,
            snapshotSessionOptionsName,
            testMode);
    }
}
