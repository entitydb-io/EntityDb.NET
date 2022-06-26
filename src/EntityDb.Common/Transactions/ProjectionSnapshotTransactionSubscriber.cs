using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Common.Annotations;
using EntityDb.Common.Projections;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions;

internal class ProjectionSnapshotTransactionSubscriber<TProjection> : SnapshotTransactionSubscriberBase<TProjection>
    where TProjection : IProjection<TProjection>
{
    private readonly IProjectionStrategy<TProjection> _projectionStrategy;

    public ProjectionSnapshotTransactionSubscriber
    (
        IProjectionStrategy<TProjection> projectionStrategy,
        ISnapshotRepositoryFactory<TProjection> snapshotRepositoryFactory,
        string snapshotSessionOptionsName,
        bool testMode
    ) : base(snapshotRepositoryFactory, snapshotSessionOptionsName, testMode)
    {
        _projectionStrategy = projectionStrategy;
    }

    protected override async Task<(TProjection? previousLatestSnapshot, TProjection nextSnapshot)?> GetSnapshots(ITransaction transaction, ITransactionStep transactionStep, ISnapshotRepository<TProjection> snapshotRepository)
    {
        if (transactionStep is not IAppendCommandTransactionStep appendCommandTransactionStep)
        {
            return null;
        }

        var projectionId = await _projectionStrategy.GetProjectionId(appendCommandTransactionStep.EntityId, appendCommandTransactionStep.Entity);

        var previousLatestSnapshot = await snapshotRepository.GetSnapshot(projectionId);

        var annotatedCommand = EntityAnnotation<object>.CreateFromBoxedData
        (
            transaction.Id,
            transaction.TimeStamp,
            appendCommandTransactionStep.EntityId,
            appendCommandTransactionStep.EntityVersionNumber,
            appendCommandTransactionStep.Command
        );

        var nextSnapshot = (previousLatestSnapshot ?? TProjection.Construct(projectionId)).Reduce(annotatedCommand);

        return (previousLatestSnapshot, nextSnapshot);
    }

    public static ProjectionSnapshotTransactionSubscriber<TProjection> Create(IServiceProvider serviceProvider,
        string snapshotSessionOptionsName, bool testMode)
    {
        return ActivatorUtilities.CreateInstance<ProjectionSnapshotTransactionSubscriber<TProjection>>(serviceProvider,
            snapshotSessionOptionsName,
            testMode);
    }
}
