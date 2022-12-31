using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Queries;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

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

    public Id? GetProjectionIdOrDefault(ITransaction transaction, ITransactionCommand transactionCommand)
    {
        return TProjection.GetProjectionIdOrDefault(transaction, transactionCommand);
    }

    public async Task<TProjection> GetSnapshot(Pointer projectionPointer, CancellationToken cancellationToken = default)
    {
        var projection = SnapshotRepository is not null
            ? await SnapshotRepository.GetSnapshotOrDefault(projectionPointer, cancellationToken) ??
              TProjection.Construct(projectionPointer.Id)
            : TProjection.Construct(projectionPointer.Id);

        var newTransactionIdsQuery = await projection.GetCommandQuery(projectionPointer, TransactionRepository, cancellationToken);

        var transactions = TransactionRepository.EnumerateTransactions(newTransactionIdsQuery, cancellationToken);

        await foreach (var transaction in transactions)
        {
            foreach (var transactionCommand in transaction.Commands)
            {
                projection = projection.Reduce(transaction, transactionCommand);
            }
        }

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
        if (snapshotRepository is null)
        {
            return ActivatorUtilities.CreateInstance<ProjectionRepository<TProjection>>(serviceProvider,
                transactionRepository);
        }

        return ActivatorUtilities.CreateInstance<ProjectionRepository<TProjection>>(serviceProvider,
            transactionRepository, snapshotRepository);
    }
}
