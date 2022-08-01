using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Entities;

internal class EntityRepository<TEntity> : DisposableResourceBaseClass, IEntityRepository<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IEnumerable<ITransactionSubscriber> _transactionSubscribers;

    public EntityRepository
    (
        IEnumerable<ITransactionSubscriber> transactionSubscribers,
        ITransactionRepository transactionRepository,
        ISnapshotRepository<TEntity>? snapshotRepository = null
    )
    {
        _transactionSubscribers = transactionSubscribers;
        TransactionRepository = transactionRepository;
        SnapshotRepository = snapshotRepository;
    }

    public ITransactionRepository TransactionRepository { get; }
    public ISnapshotRepository<TEntity>? SnapshotRepository { get; }

    public async Task<TEntity> GetSnapshot(Pointer entityPointer, CancellationToken cancellationToken = default)
    {
        var snapshot = SnapshotRepository is not null
            ? await SnapshotRepository.GetSnapshotOrDefault(entityPointer, cancellationToken) ??
              TEntity.Construct(entityPointer.Id)
            : TEntity.Construct(entityPointer.Id);

        var commandQuery = new GetEntityCommandsQuery(entityPointer, snapshot.GetVersionNumber());

        var commands = TransactionRepository.EnumerateCommands(commandQuery, cancellationToken);

        var entity = snapshot;

        await foreach (var command in commands)
        {
            entity = entity.Reduce(command);
        }

        if (!entityPointer.IsSatisfiedBy(entity.GetVersionNumber()))
        {
            throw new SnapshotPointerDoesNotExistException();
        }

        return entity;
    }

    public async Task<bool> PutTransaction(ITransaction transaction, CancellationToken cancellationToken = default)
    {
        try
        {
            return await TransactionRepository.PutTransaction(transaction, cancellationToken);
        }
        finally
        {
            Publish(transaction);
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await TransactionRepository.DisposeAsync();

        if (SnapshotRepository is not null)
        {
            await SnapshotRepository.DisposeAsync();
        }
    }

    private void Publish(ITransaction transaction)
    {
        foreach (var transactionSubscriber in _transactionSubscribers)
        {
            transactionSubscriber.Notify(transaction);
        }
    }

    public static EntityRepository<TEntity> Create
    (
        IServiceProvider serviceProvider,
        ITransactionRepository transactionRepository,
        ISnapshotRepository<TEntity>? snapshotRepository = null
    )
    {
        if (snapshotRepository is null)
        {
            return ActivatorUtilities.CreateInstance<EntityRepository<TEntity>>(serviceProvider,
                transactionRepository);
        }

        return ActivatorUtilities.CreateInstance<EntityRepository<TEntity>>(serviceProvider, transactionRepository,
            snapshotRepository);
    }
}
