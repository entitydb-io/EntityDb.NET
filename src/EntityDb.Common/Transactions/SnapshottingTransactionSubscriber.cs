using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions
{
    internal class SnapshottingTransactionSubscriber<TEntity> : AsyncTransactionSubscriber<TEntity>
    {
        private readonly ISnapshotRepositoryFactory<TEntity> _snapshotRepositoryFactory;
        private readonly ISnapshotSessionOptions _snapshotSessionOptions = new SnapshotSessionOptions();

        public SnapshottingTransactionSubscriber
        (
            ISnapshotRepositoryFactory<TEntity> snapshotRepositoryFactory,
            bool testMode
        ) : base(testMode)
        {
            _snapshotRepositoryFactory = snapshotRepositoryFactory;
        }

        protected override async Task NotifyAsync(ITransaction<TEntity> transaction)
        {
            var commandGroups = transaction.Commands
                .GroupBy(command => command.EntityId);
                
            foreach (var commandGroup in commandGroups)
            {
                var entityId = commandGroup.Key;
                var nextSnapshot = commandGroup.Last().NextEntitySnapshot;
            
                await using var snapshotRepository =
                    await _snapshotRepositoryFactory.CreateRepository(_snapshotSessionOptions);
            
                await snapshotRepository.PutSnapshot(entityId, nextSnapshot);
            }
        }

        public static SnapshottingTransactionSubscriber<TEntity> Create(IServiceProvider serviceProvider, bool testMode)
        {
            return ActivatorUtilities.CreateInstance<SnapshottingTransactionSubscriber<TEntity>>(serviceProvider,
                testMode);
        }
    }
}
