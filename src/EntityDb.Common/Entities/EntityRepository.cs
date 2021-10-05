using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.Common.Entities
{
    internal class EntityRepository<TEntity> : IEntityRepository<TEntity>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITransactionRepository<TEntity> _transactionRepository;
        private readonly ISnapshotRepository<TEntity>? _snapshotRepository;

        private EntityRepository
        (
            IServiceProvider serviceProvider,
            ITransactionRepository<TEntity> transactionRepository,
            ISnapshotRepository<TEntity>? snapshotRepository = null
        )
        {
            _serviceProvider = serviceProvider;
            _transactionRepository = transactionRepository;
            _snapshotRepository = snapshotRepository;
        }
        
        public async Task<TEntity> GetSnapshotOrConstruct(Guid entityId)
        {
            TEntity? snapshot = default;

            if (_snapshotRepository != null)
            {
                snapshot = await _snapshotRepository.GetSnapshot(entityId);
            }

            return snapshot ?? _serviceProvider.Construct<TEntity>(entityId);
        }

        public async Task<TEntity> GetCurrentOrConstruct(Guid entityId)
        {
            var entity = await GetSnapshotOrConstruct(entityId);

            var versionNumber = _serviceProvider.GetVersionNumber(entity);

            var factQuery = new GetEntityQuery(entityId, versionNumber);

            var facts = await _transactionRepository.GetFacts(factQuery);

            entity = entity.Reduce(facts);

            return entity;
        }

        public Task<bool> PutTransaction(ITransaction<TEntity> transaction)
        {
            return _transactionRepository.PutTransaction(transaction);
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            await _transactionRepository.DisposeAsync();

            if (_snapshotRepository != null)
            {
                await _snapshotRepository.DisposeAsync();
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
                return ActivatorUtilities.CreateInstance<EntityRepository<TEntity>>(serviceProvider, transactionRepository);
            }
            
            return ActivatorUtilities.CreateInstance<EntityRepository<TEntity>>(serviceProvider, transactionRepository,
                snapshotRepository);
        }
    }
}
