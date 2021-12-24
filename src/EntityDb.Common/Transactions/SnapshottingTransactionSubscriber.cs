using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions
{
    internal class SnapshottingTransactionSubscriber<TEntity> : TransactionSubscriber<TEntity>
    {
        private readonly ISnapshotRepositoryFactory<TEntity> _snapshotRepositoryFactory;
        private readonly string _snapshotSessionOptionsName;

        public SnapshottingTransactionSubscriber
        (
            ISnapshotRepositoryFactory<TEntity> snapshotRepositoryFactory,
            string snapshotSessionOptionsName,
            bool synchronousMode
        ) : base(synchronousMode)
        {
            _snapshotRepositoryFactory = snapshotRepositoryFactory;
            _snapshotSessionOptionsName = snapshotSessionOptionsName;
        }

        protected override async Task NotifyAsync(ITransaction<TEntity> transaction)
        {
            var snapshotRepository =
                await _snapshotRepositoryFactory.CreateRepository(_snapshotSessionOptionsName);

            var commandGroups = transaction.Steps
                .GroupBy(command => command.EntityId);

            foreach(var commandGroup in commandGroups)
            {
                var entityId = commandGroup.Key;
                var nextSnapshot = commandGroup.Last().NextEntitySnapshot;

                await snapshotRepository.PutSnapshot(entityId, nextSnapshot);
            }
        }

        public static SnapshottingTransactionSubscriber<TEntity> Create(IServiceProvider serviceProvider,
            string snapshotSessionOptionsName, bool synchronousMode)
        {
            return ActivatorUtilities.CreateInstance<SnapshottingTransactionSubscriber<TEntity>>(serviceProvider,
                snapshotSessionOptionsName,
                synchronousMode);
        }
    }
}
