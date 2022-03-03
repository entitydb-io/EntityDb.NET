using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.Common.Entities;

internal class EntityRepository<TEntity> : DisposableResourceBaseClass, IEntityRepository<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IEnumerable<ITransactionSubscriber> _transactionSubscribers;
    
    public ITransactionRepository TransactionRepository { get; }
    public ISnapshotRepository<TEntity>? SnapshotRepository { get; }

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

    public async Task<TEntity> GetCurrent(Id entityId)
    {
        var entity = await SnapshotRepository.GetSnapshotOrDefault(entityId) ?? TEntity.Construct(entityId);

        var versionNumber = entity.GetVersionNumber();

        var commandQuery = new GetCurrentEntityQuery(entityId, versionNumber);

        var commands = await TransactionRepository.GetCommands(commandQuery);

        entity = entity.Reduce(commands);

        if (entity.GetVersionNumber() == VersionNumber.MinValue)
        {
            throw new EntityNotCreatedException();
        }

        return entity;
    }

    public async Task<TEntity> GetAtVersion(Id entityId, VersionNumber lteVersionNumber)
    {
        var commandQuery = new GetEntityAtVersionQuery(entityId, lteVersionNumber);

        var commands = await TransactionRepository.GetCommands(commandQuery);

        return TEntity
            .Construct(entityId)
            .Reduce(commands);
    }

    public async Task<bool> PutTransaction(ITransaction transaction)
    {
        try
        {
            return await TransactionRepository.PutTransaction(transaction);
        }
        finally
        {
            Publish(transaction);
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await TransactionRepository.DisposeAsync();

        if (SnapshotRepository != null)
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
        if (snapshotRepository == null)
        {
            return ActivatorUtilities.CreateInstance<EntityRepository<TEntity>>(serviceProvider,
                transactionRepository);
        }

        return ActivatorUtilities.CreateInstance<EntityRepository<TEntity>>(serviceProvider, transactionRepository,
            snapshotRepository);
    }
}
