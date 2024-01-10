using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Sources;
using EntityDb.Common.Sources.Processors;
using EntityDb.Common.Sources.Processors.Queues;

namespace EntityDb.Common.Sources.Subscribers;

internal class ProjectionStateSourceSubscriber<TProjection> : ISourceSubscriber
    where TProjection : IProjection<TProjection>
{
    private readonly ISourceProcessorQueue _sourceProcessorQueue;

    public ProjectionStateSourceSubscriber(ISourceProcessorQueue sourceProcessorQueue)
    {
        _sourceProcessorQueue = sourceProcessorQueue;
    }

    public void Notify(Source source)
    {
        if (!TProjection.EnumerateRelevantStateIds(source).Any())
        {
            return;
        }

        _sourceProcessorQueue.Enqueue<ProjectionStateSourceProcessor<TProjection>>(source);
    }
}
