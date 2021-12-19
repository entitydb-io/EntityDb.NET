using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Entities
{
    internal class EntityRepositoryFactory<TEntity> : IEntityRepositoryFactory<TEntity>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITransactionRepositoryFactory<TEntity> _transactionRepositoryFactory;
        private readonly ISnapshotRepositoryFactory<TEntity>? _snapshotRepositoryFactory;

        public EntityRepositoryFactory(IServiceProvider serviceProvider, ITransactionRepositoryFactory<TEntity> transactionRepositoryFactory,
            ISnapshotRepositoryFactory<TEntity>? snapshotRepositoryFactory = null)
        {
            _serviceProvider = serviceProvider;
            _transactionRepositoryFactory = transactionRepositoryFactory;
            _snapshotRepositoryFactory = snapshotRepositoryFactory;
        }

        public async Task<IEntityRepository<TEntity>> CreateRepository(string transactionSessionOptionsName,
            string? snapshotSessionOptionsName = null)
        {
            var transactionRepository = await _transactionRepositoryFactory.CreateRepository(transactionSessionOptionsName);

            if (_snapshotRepositoryFactory == null || snapshotSessionOptionsName == null)
            {
                return ActivatorUtilities.CreateInstance<EntityRepository<TEntity>>(_serviceProvider,
                    transactionRepository);
            }

            var snapshotRepository = await _snapshotRepositoryFactory.CreateRepository(snapshotSessionOptionsName);

            return ActivatorUtilities.CreateInstance<EntityRepository<TEntity>>(_serviceProvider, transactionRepository, snapshotRepository);
        }
    }
}
