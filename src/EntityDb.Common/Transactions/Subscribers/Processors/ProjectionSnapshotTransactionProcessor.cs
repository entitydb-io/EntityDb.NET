using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Projections;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

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

        var snapshotTransactionCommandProcessorCache = new SnapshotTransactionCommandProcessorCache<TProjection>();

        var projectionSnapshotTransactionCommandProcessor = new ProjectionSnapshotTransactionCommandProcessor<TProjection>
        (
            projectionRepository,
            snapshotTransactionCommandProcessorCache
        );

        await ProcessTransactionCommands
        (
            projectionRepository.SnapshotRepository,
            snapshotTransactionCommandProcessorCache,
            transaction,
            projectionSnapshotTransactionCommandProcessor,
            cancellationToken
        );
    }

    public static ProjectionSnapshotTransactionProcessor<TProjection> Create(IServiceProvider serviceProvider,
        string transactionSessionOptionsName, string snapshotSessionOptionsName)
    {
        return ActivatorUtilities.CreateInstance<ProjectionSnapshotTransactionProcessor<TProjection>>(serviceProvider,
            transactionSessionOptionsName,
            snapshotSessionOptionsName);
    }
}
