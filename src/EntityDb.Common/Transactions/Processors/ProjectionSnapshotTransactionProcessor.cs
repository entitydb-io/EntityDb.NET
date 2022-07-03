using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Annotations;
using EntityDb.Common.Projections;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions.Processors;

internal sealed class
    ProjectionSnapshotTransactionProcessor<TProjection> : SnapshotTransactionProcessorBase<TProjection>
    where TProjection : IProjection<TProjection>
{
    private readonly IProjectionRepositoryFactory<TProjection> _projectionRepositoryFactory;
    private readonly string _snapshotSessionOptionsName;
    private readonly string _transactionSessionOptionsName;

    public ProjectionSnapshotTransactionProcessor
    (
        IProjectionRepositoryFactory<TProjection> projectionRepositoryFactory,
        string transactionSessionOptionsName,
        string snapshotSessionOptionsName
    )
    {
        _projectionRepositoryFactory = projectionRepositoryFactory;
        _transactionSessionOptionsName = transactionSessionOptionsName;
        _snapshotSessionOptionsName = snapshotSessionOptionsName;
    }

    public override async Task ProcessTransaction(ITransaction transaction, CancellationToken cancellationToken)
    {
        await using var projectionRepository = await _projectionRepositoryFactory.CreateRepository(
            _transactionSessionOptionsName, _snapshotSessionOptionsName, cancellationToken);

        if (projectionRepository.SnapshotRepository is null)
        {
            return;
        }

        var snapshotCache = CreateSnapshotCache();

        async Task<(TProjection?, TProjection)?> GetSnapshots(
            IAppendCommandTransactionStep appendCommandTransactionStep)
        {
            var projectionId = projectionRepository.GetProjectionIdOrDefault(appendCommandTransactionStep.Entity);

            if (projectionId is null)
            {
                return null;
            }

            var previousLatestPointer = projectionId.Value + appendCommandTransactionStep.PreviousEntityVersionNumber;

            TProjection? previousLatestSnapshot = default;

            if (previousLatestPointer.VersionNumber != VersionNumber.MinValue)
            {
                previousLatestSnapshot = snapshotCache.GetSnapshotOrDefault(previousLatestPointer) ??
                                         await projectionRepository.GetSnapshot(previousLatestPointer,
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

        await ProcessTransactionSteps(projectionRepository.SnapshotRepository, snapshotCache, transaction, GetSnapshots,
            cancellationToken);
    }

    public static ProjectionSnapshotTransactionProcessor<TProjection> Create(IServiceProvider serviceProvider,
        string transactionSessionOptionsName, string snapshotSessionOptionsName)
    {
        return ActivatorUtilities.CreateInstance<ProjectionSnapshotTransactionProcessor<TProjection>>(serviceProvider,
            transactionSessionOptionsName,
            snapshotSessionOptionsName);
    }
}
