using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Projections;

internal sealed class ProjectionRepository<TProjection> : DisposableResourceBaseClass, IProjectionRepository<TProjection>
    where TProjection : IProjection<TProjection>
{
    public IProjectionStrategy<TProjection> ProjectionStrategy { get; }
    public ITransactionRepository TransactionRepository { get; }
    public ISnapshotRepository<TProjection> SnapshotRepository { get; }
    
    public ProjectionRepository
    (
        IProjectionStrategy<TProjection> projectionStrategy,
        ISnapshotRepository<TProjection> snapshotRepository,
        ITransactionRepository transactionRepository
    )
    {
        ProjectionStrategy = projectionStrategy;
        TransactionRepository = transactionRepository;
        SnapshotRepository = snapshotRepository;
    }

    public async Task<TProjection> GetCurrent(Id projectionId)
    {
        var projection = await SnapshotRepository.GetSnapshot(projectionId) ?? TProjection.Construct(projectionId);

        var entityIds = await ProjectionStrategy.GetEntityIds(projectionId, projection);

        if (entityIds.Length == 0)
        {
            return projection;
        }
        
        foreach (var entityId in entityIds)
        {
            var entityVersionNumber = projection.GetEntityVersionNumber(entityId);
            
            var commandQuery = new GetCurrentEntityQuery(entityId, entityVersionNumber);

            var annotatedCommands = await TransactionRepository.GetAnnotatedCommands(commandQuery);

            projection = projection.Reduce(annotatedCommands);
        }

        return projection;
    }
    
    public static ProjectionRepository<TProjection> Create
    (
        IServiceProvider serviceProvider,
        ITransactionRepository transactionRepository,
        ISnapshotRepository<TProjection> snapshotRepository
    )
    {
        return ActivatorUtilities.CreateInstance<ProjectionRepository<TProjection>>(serviceProvider,
            transactionRepository, snapshotRepository);
    }
}
