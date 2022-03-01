using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Statements;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using System;
using System.Linq;

namespace EntityDb.Common.Projections;

internal sealed class StatementProjectionRepository<TProjection, TEntity> : ProjectionRepositoryBase<TProjection, TEntity>, IProjectionRepository<TProjection>
    where TProjection : IStatementProjection<TProjection>
{
    public StatementProjectionRepository
    (
        IProjectionStrategy<TProjection> projectionStrategy,
        ISnapshotRepository<TProjection> snapshotRepository,
        ITransactionRepository<TEntity> transactionRepository
    ) : base(projectionStrategy, snapshotRepository, transactionRepository)
    {   
    }

    protected override TProjection Construct(Guid projectionId)
    {
        return TProjection.Construct(projectionId);
    }

    protected override ulong GetEntityVersionNumber(TProjection projection, Guid entityId)
    {
        return projection.GetEntityVersionNumber(entityId);
    }

    protected override TProjection Reduce(TProjection projection, Guid entityId, ICommand<TEntity>[] commands)
    {
        var statements = commands
            .Where(command => command is IStatement<TProjection>)
            .Cast<IStatement<TProjection>>();
            
        var skipCount = commands
            .Count(command => command is not IStatement<TProjection>);

        return projection
            .Reduce(entityId, statements)
            .SkipEntityVersionNumbers(entityId, skipCount);
    }
}
