using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Queries;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Projections;

internal abstract class ProjectionRepositoryBase<TProjection, TEntity>
{
    private readonly IProjectionStrategy<TProjection> _projectionStrategy;
    private readonly ITransactionRepository<TEntity> _transactionRepository;
    
    public ISnapshotRepository<TProjection> SnapshotRepository { get; }
    
    protected ProjectionRepositoryBase
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

    protected abstract TProjection Construct(Guid projectionId);

    protected abstract ulong GetEntityVersionNumber(TProjection projection, Guid entityId);
    
    protected abstract TProjection Reduce(TProjection projection, Guid entityId, ICommand<TEntity>[] commands);
    
    public async Task<TProjection> GetCurrent(Guid projectionId)
    {
        var projection = await SnapshotRepository.GetSnapshot(projectionId) ?? Construct(projectionId);
        
        var entityIds = await _projectionStrategy.GetEntityIds(projectionId, projection);
        
        foreach (var entityId in entityIds)
        {
            var entityVersionNumber = GetEntityVersionNumber(projection, entityId);
            
            var commandQuery = new GetCurrentEntityQuery(entityId, entityVersionNumber);

            var commands = await _transactionRepository.GetCommands(commandQuery);

            projection = Reduce(projection, entityId, commands);
        }
    }
}
