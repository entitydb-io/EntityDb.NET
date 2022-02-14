using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Projections
{

    internal class ProjectionRepository<TEntity, TProjection> : IProjectionRepository<TProjection>
        where TProjection : IProjection<TProjection>
    {
        private readonly IProjectionStrategy<TProjection> _projectingStrategy;
        private readonly ITransactionRepository<TEntity> _transactionRepository;
        private readonly ISnapshotRepository<TProjection> _snapshotRepository;

        public ProjectionRepository(IProjectionStrategy<TProjection> projectingStrategy, ITransactionRepository<TEntity> transactionRepository, ISnapshotRepository<TProjection> snapshotRepository)
        {
            _projectingStrategy = projectingStrategy;
            _transactionRepository = transactionRepository;
            _snapshotRepository = snapshotRepository;
        }

        public ISnapshotRepository<TProjection> SnapshotRepository => _snapshotRepository;

        public async Task<TProjection> GetCurrent(Guid projectionId)
        {
            var projection = await _snapshotRepository.GetSnapshotOrDefault(projectionId);

            var entityIds = await _projectingStrategy.GetEntityIds(projectionId, projection);

            projection ??= TProjection.Construct(projectionId);

            foreach (var entityId in entityIds)
            {
                var versionNumber = projection.GetEntityVersionNumber(entityId);

                var commandQuery = new GetCurrentEntityQuery(entityId, versionNumber);

                var commands = await _transactionRepository.GetCommands(commandQuery);

                projection = projection.Reduce(entityId, commands);

                if (projection.GetEntityVersionNumber(entityId) == 0)
                {
                    throw new EntityNotCreatedException();
                }
            }

            return projection;
        }

        public async ValueTask DisposeAsync()
        {
            await _transactionRepository.DisposeAsync();
            await _snapshotRepository.DisposeAsync();
        }

        public static ProjectionRepository<TEntity, TProjection> Create
        (
            IServiceProvider serviceProvider,
            ITransactionRepository<TEntity> transactionRepository,
            ISnapshotRepository<TProjection> snapshotRepository
        )
        {
            return ActivatorUtilities.CreateInstance<ProjectionRepository<TEntity, TProjection>>(serviceProvider, transactionRepository,
                snapshotRepository);
        }
    }
}
