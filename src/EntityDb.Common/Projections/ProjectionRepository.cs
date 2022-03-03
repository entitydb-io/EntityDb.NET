using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Disposables;
using EntityDb.Common.Queries;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Projections;

internal sealed class ProjectionRepository<TProjection, TEntity> : DisposableResourceBaseClass, IProjectionRepository<TProjection>
    where TProjection : IProjection<TProjection>
{
    private readonly IProjectionStrategy<TProjection> _projectionStrategy;
    private readonly ITransactionRepository<TEntity> _transactionRepository;
    
    public ISnapshotRepository<TProjection> SnapshotRepository { get; }
    
    public ProjectionRepository
    (
        IProjectionStrategy<TProjection> projectionStrategy,
        ISnapshotRepository<TProjection> snapshotRepository,
        ITransactionRepository<TEntity> transactionRepository
    )
    {
        _projectionStrategy = projectionStrategy;
        _transactionRepository = transactionRepository;
        
        SnapshotRepository = snapshotRepository;
    }

    public async Task<TProjection> GetCurrent(Guid projectionId)
    {
        var projection = await SnapshotRepository.GetSnapshot(projectionId) ?? TProjection.Construct(projectionId);
        
        var entityIds = await _projectionStrategy.GetEntityIds(projectionId, projection);
        
        foreach (var entityId in entityIds)
        {
            var entityVersionNumber = projection.GetEntityVersionNumber(entityId);
            
            var commandQuery = new GetCurrentEntityQuery(entityId, entityVersionNumber);

            var commands = await _transactionRepository.GetCommands(commandQuery);

            projection = projection.Reduce(entityId, commands);
        }
        
        //TODO: Store the new projection as a snapshot if it changed
        
        //TODO: Don't return default
        return default!;
    }
}
