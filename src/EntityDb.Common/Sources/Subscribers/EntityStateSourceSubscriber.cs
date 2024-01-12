using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Common.Sources.Processors;
using EntityDb.Common.Sources.Processors.Queues;

namespace EntityDb.Common.Sources.Subscribers;

internal sealed class EntityStateSourceSubscriber<TEntity> : ISourceSubscriber
    where TEntity : IEntity<TEntity>
{
    private readonly ISourceProcessorQueue _sourceProcessorQueue;

    public EntityStateSourceSubscriber(ISourceProcessorQueue sourceProcessorQueue)
    {
        _sourceProcessorQueue = sourceProcessorQueue;
    }

    public void Notify(Source source)
    {
        if (!source.Messages.Any(message => TEntity.CanReduce(message.Delta)))
        {
            return;
        }

        _sourceProcessorQueue.Enqueue<EntityStateSourceProcessor<TEntity>>(source);
    }
}
