using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.Common.Entities
{
    internal class EntityRepository<TEntity> : IEntityRepository<TEntity>
        where TEntity : IEntity<TEntity>
    {
        private readonly ISnapshotRepository<TEntity>? _snapshotRepository;
        private readonly ITransactionRepository<TEntity> _transactionRepository;
        private readonly IEnumerable<ITransactionSubscriber<TEntity>> _transactionSubscribers;

        public EntityRepository
        (
            IEnumerable<ITransactionSubscriber<TEntity>> transactionSubscribers,
            ITransactionRepository<TEntity> transactionRepository,
            ISnapshotRepository<TEntity>? snapshotRepository = null
        )
        {
            _transactionSubscribers = transactionSubscribers;
            _transactionRepository = transactionRepository;
            _snapshotRepository = snapshotRepository;
        }

        public ITransactionRepository<TEntity> TransactionRepository => _transactionRepository;

        public ISnapshotRepository<TEntity>? SnapshotRepository => _snapshotRepository;

        public async Task<TEntity?> GetSnapshotOrDefault(Guid entityId)
        {
            if (_snapshotRepository != null)
            {
                return await _snapshotRepository.GetSnapshot(entityId);
            }

            return default;
        }

        public async Task<TEntity> GetCurrent(Guid entityId)
        {
            var snapshot = await GetSnapshotOrDefault(entityId);

            var entity = snapshot ?? TEntity.Construct(entityId);

            var versionNumber = entity.GetVersionNumber();

            var commandQuery = new GetCurrentEntityQuery(entityId, versionNumber);

            var commands = await _transactionRepository.GetCommands(commandQuery);

            entity = entity.Reduce(commands);

            if (entity.GetVersionNumber() == 0)
            {
                throw new EntityNotCreatedException();
            }

            return entity;
        }

        public async Task<TEntity> GetAtVersion(Guid entityId, ulong lteVersionNumber)
        {
            var commandQuery = new GetEntityAtVersionQuery(entityId, lteVersionNumber);

            var commands = await _transactionRepository.GetCommands(commandQuery);

            return TEntity
                .Construct(entityId)
                .Reduce(commands);
        }

        public async Task<bool> PutTransaction(ITransaction<TEntity> transaction)
        {
            try
            {
                return await _transactionRepository.PutTransaction(transaction);
            }
            finally
            {
                Publish(transaction);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _transactionRepository.DisposeAsync();

            if (_snapshotRepository != null)
            {
                await _snapshotRepository.DisposeAsync();
            }
        }

        private void Publish(ITransaction<TEntity> transaction)
        {
            foreach (var transactionSubscriber in _transactionSubscribers)
            {
                transactionSubscriber.Notify(transaction);
            }
        }

        public static EntityRepository<TEntity> Create
        (
            IServiceProvider serviceProvider,
            ITransactionRepository<TEntity> transactionRepository,
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
}
