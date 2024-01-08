using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Common.Extensions;
using EntityDb.Common.Sources.Processors;
using EntityDb.Common.Sources.Processors.Queues;

namespace EntityDb.Common.Sources.Subscribers;

internal class EntitySnapshotSourceSubscriber<TEntity> : ISourceSubscriber
    where TEntity : IEntity<TEntity>
{
    private readonly ISourceProcessorQueue _sourceProcessorQueue;

    public EntitySnapshotSourceSubscriber(ISourceProcessorQueue sourceProcessorQueue)
    {
        _sourceProcessorQueue = sourceProcessorQueue;
    }

    public void Notify(Source source)
    {
        if (!source.Messages.Any(message => TEntity.CanReduce(message.Delta)))
        {
            return;
        }

        _sourceProcessorQueue.Enqueue<EntitySnapshotSourceProcessor<TEntity>>(source);
    }
}
