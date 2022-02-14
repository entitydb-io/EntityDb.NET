using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Projections
{
    internal class ProjectionRepositoryFactory<TEntity, TProjection> : IProjectionRepositoryFactory<TProjection>
        where TProjection : IProjection<TProjection>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITransactionRepositoryFactory<TEntity> _transactionRepositoryFactory;
        private readonly ISnapshotRepositoryFactory<TProjection> _snapshotRepositoryFactory;

        public ProjectionRepositoryFactory(IServiceProvider serviceProvider,
            ITransactionRepositoryFactory<TEntity> transactionRepositoryFactory,
            ISnapshotRepositoryFactory<TProjection> snapshotRepositoryFactory)
        {
            _serviceProvider = serviceProvider;
            _transactionRepositoryFactory = transactionRepositoryFactory;
            _snapshotRepositoryFactory = snapshotRepositoryFactory;
        }

        public async Task<IProjectionRepository<TProjection>> CreateRepository(string transactionSessionOptionsName,
            string snapshotSessionOptionsName)
        {
            var transactionRepository =
                await _transactionRepositoryFactory.CreateRepository(transactionSessionOptionsName);

            var snapshotRepository =
                await _snapshotRepositoryFactory.CreateRepository(snapshotSessionOptionsName);

            return ProjectionRepository<TEntity, TProjection>.Create(_serviceProvider,
                transactionRepository, snapshotRepository);
        }
    }
}
