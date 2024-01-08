using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Sources;
using EntityDb.Common.Extensions;
using EntityDb.Common.Sources.Processors;
using EntityDb.Common.Sources.Processors.Queues;

namespace EntityDb.Common.Sources.Subscribers;

internal class ProjectionSnapshotSourceSubscriber<TProjection> : ISourceSubscriber
    where TProjection : IProjection<TProjection>
{
    private readonly ISourceProcessorQueue _sourceProcessorQueue;

    public ProjectionSnapshotSourceSubscriber(ISourceProcessorQueue sourceProcessorQueue)
    {
        _sourceProcessorQueue = sourceProcessorQueue;
    }

    public void Notify(Source source)
    {
        if (!TProjection.EnumerateEntityIds(source).Any())
        {
            return;
        }

        _sourceProcessorQueue.Enqueue<ProjectionSnapshotSourceProcessor<TProjection>>(source);
    }
}
