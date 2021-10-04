using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Queries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Common.Entities
{
    internal class EntityRepository<TEntity> : IEntityRepository<TEntity>
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityRepository
        (
            IServiceProvider serviceProvider,
            ITransactionRepository<TEntity> transactionRepository,
            ISnapshotRepository<TEntity>? snapshotRepository = null
        )
        {
            _serviceProvider = serviceProvider;
            TransactionRepository = transactionRepository;
            SnapshotRepository = snapshotRepository;
        }

        public ITransactionRepository<TEntity> TransactionRepository { get; }

        public ISnapshotRepository<TEntity>? SnapshotRepository { get; }

        public async Task<TEntity> Get(Guid entityId)
        {
            TEntity? snapshot = default;

            if (SnapshotRepository != null)
            {
                snapshot = await SnapshotRepository.GetSnapshot(entityId);
            }

            TEntity? entity = snapshot ?? _serviceProvider.Construct<TEntity>(entityId);

            ulong versionNumber = _serviceProvider.GetVersionNumber(entity);

            GetEntityQuery? factQuery = new GetEntityQuery(entityId, versionNumber);

            IFact<TEntity>[]? facts = await TransactionRepository.GetFacts(factQuery);

            entity = entity.Reduce(facts);

            return entity;
        }

        public Task<bool> Put(ITransaction<TEntity> transaction)
        {
            if (SnapshotRepository != null)
            {
                IEnumerable<ITransactionCommand<TEntity>>? lastCommands = transaction.Commands
                    .GroupBy(command => command.EntityId)
                    .Select(group => group.Last());

                foreach (var lastCommand in lastCommands)
                {
                    if (_serviceProvider.ShouldPutSnapshot(lastCommand.PreviousSnapshot, lastCommand.NextSnapshot))
                    {
                        SnapshotRepository.PutSnapshot(lastCommand.EntityId, lastCommand.NextSnapshot);
                    }
                }
            }

            return TransactionRepository.PutTransaction(transaction);
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            await TransactionRepository.DisposeAsync();

            if (SnapshotRepository != null)
            {
                await SnapshotRepository.DisposeAsync();
            }
        }
    }
}
