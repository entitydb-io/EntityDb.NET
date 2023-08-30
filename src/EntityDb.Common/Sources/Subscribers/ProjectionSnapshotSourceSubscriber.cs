using EntityDb.Abstractions.Sources;
using EntityDb.Common.Extensions;
using EntityDb.Common.Projections;
using EntityDb.Common.Sources.Processors;
using EntityDb.Common.Sources.Processors.Queues;

namespace EntityDb.Common.Sources.Subscribers;

internal class ProjectionSnapshotSourceSubscriber<TProjection> : ISourceSubscriber
    where TProjection : IProjection<TProjection>
{
    private readonly ISourceProcessorQueue _transactionProcessorQueue;

    public ProjectionSnapshotSourceSubscriber(ISourceProcessorQueue transactionProcessorQueue)
    {
        _transactionProcessorQueue = transactionProcessorQueue;
    }

    public void Notify(ISource source)
    {
        if (!TProjection.EnumerateProjectionIds(source).Any())
        {
            return;
        }

        _transactionProcessorQueue.Enqueue<ProjectionSnapshotSourceProcessor<TProjection>>(source);
    }
}
