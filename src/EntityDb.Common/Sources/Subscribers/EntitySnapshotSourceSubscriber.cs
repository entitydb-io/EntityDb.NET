using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Entities;
using EntityDb.Common.Extensions;
using EntityDb.Common.Sources.Processors;
using EntityDb.Common.Sources.Processors.Queues;

namespace EntityDb.Common.Sources.Subscribers;

internal class EntitySnapshotSourceSubscriber<TEntity> : ISourceSubscriber
    where TEntity : IEntity<TEntity>
{
    private readonly ISourceProcessorQueue _transactionProcessorQueue;

    public EntitySnapshotSourceSubscriber(ISourceProcessorQueue transactionProcessorQueue)
    {
        _transactionProcessorQueue = transactionProcessorQueue;
    }

    public void Notify(ISource source)
    {
        if (source is not ITransaction transaction || !transaction.Commands.Any(command => TEntity.CanReduce(command.Data)))
        {
            return;
        }

        _transactionProcessorQueue.Enqueue<EntitySnapshotSourceProcessor<TEntity>>(source);
    }
}
