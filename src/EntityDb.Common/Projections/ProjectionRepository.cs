using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Projections;

internal sealed class ProjectionRepository<TProjection> : DisposableResourceBaseClass,
    IProjectionRepository<TProjection>
    where TProjection : IProjection<TProjection>
{
    public ProjectionRepository
    (
        ITransactionRepository transactionRepository,
        ISnapshotRepository<TProjection>? snapshotRepository = null
    )
    {
        TransactionRepository = transactionRepository;
        SnapshotRepository = snapshotRepository;
    }

    public ITransactionRepository TransactionRepository { get; }

    public ISnapshotRepository<TProjection>? SnapshotRepository { get; }

    public Id? GetProjectionIdOrDefault(object entity)
    {
        return TProjection.GetProjectionIdOrDefault(entity);
    }

    public async Task<TProjection> GetSnapshot(Pointer projectionPointer, CancellationToken cancellationToken = default)
    {
        var projection = SnapshotRepository is not null
            ? await SnapshotRepository.GetSnapshotOrDefault(projectionPointer, cancellationToken) ??
              TProjection.Construct(projectionPointer.Id)
            : TProjection.Construct(projectionPointer.Id);

        var commandQuery = projection.GetCommandQuery(projectionPointer);

        var annotatedCommands = await TransactionRepository.GetAnnotatedCommands(commandQuery, cancellationToken);

        projection = projection.Reduce(annotatedCommands);

        if (!projectionPointer.IsSatisfiedBy(projection.GetVersionNumber()))
        {
            throw new SnapshotPointerDoesNotExistException();
        }

        return projection;
    }

    public static IProjectionRepository<TProjection> Create(IServiceProvider serviceProvider,
        ITransactionRepository transactionRepository,
        ISnapshotRepository<TProjection>? snapshotRepository = null)
    {
        if (snapshotRepository == null)
        {
            return ActivatorUtilities.CreateInstance<ProjectionRepository<TProjection>>(serviceProvider,
                transactionRepository);
        }

        return ActivatorUtilities.CreateInstance<ProjectionRepository<TProjection>>(serviceProvider,
            transactionRepository, snapshotRepository);
    }
}
