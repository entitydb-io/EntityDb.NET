using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Transactions.Subscribers.Processors;

internal class EntitySnapshotTransactionProcessor<TEntity> : SnapshotTransactionProcessorBase<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IEntityRepositoryFactory<TEntity> _entityRepositoryFactory;
    private readonly string _snapshotSessionOptionsName;
    private readonly string _transactionSessionOptionsName;

    public EntitySnapshotTransactionProcessor
    (
        IEntityRepositoryFactory<TEntity> entityRepositoryFactory,
        string transactionSessionOptionsName,
        string snapshotSessionOptionsName
    )
    {
        _entityRepositoryFactory = entityRepositoryFactory;
        _transactionSessionOptionsName = transactionSessionOptionsName;
        _snapshotSessionOptionsName = snapshotSessionOptionsName;
    }

    public override async Task ProcessTransaction(ITransaction transaction, CancellationToken cancellationToken)
    {
        await using var entityRepository = await _entityRepositoryFactory.CreateRepository(
            _transactionSessionOptionsName, _snapshotSessionOptionsName, cancellationToken);

        if (entityRepository.SnapshotRepository is null)
        {
            return;
        }

        var snapshotTransactionStepProcessorCache = new SnapshotTransactionStepProcessorCache<TEntity>();

        var snapshotStepProcessor = new EntitySnapshotTransactionStepProcessor<TEntity>
        (
            entityRepository,
            snapshotTransactionStepProcessorCache
        );

        await ProcessTransactionSteps
        (
            entityRepository.SnapshotRepository,
            snapshotTransactionStepProcessorCache,
            transaction,
            snapshotStepProcessor,
            cancellationToken
        );
    }

    public static EntitySnapshotTransactionProcessor<TEntity> Create(IServiceProvider serviceProvider,
        string transactionSessionOptionsName, string snapshotSessionOptionsName)
    {
        return ActivatorUtilities.CreateInstance<EntitySnapshotTransactionProcessor<TEntity>>(serviceProvider,
            transactionSessionOptionsName, snapshotSessionOptionsName);
    }
}
