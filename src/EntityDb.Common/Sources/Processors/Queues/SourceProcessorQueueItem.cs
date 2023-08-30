using EntityDb.Abstractions.Sources;

namespace EntityDb.Common.Sources.Processors.Queues;

internal class SourceProcessorQueueItem : ISourceProcessorQueueItem
{
    public required Type SourceProcessorType { get; init; }
    public required ISource Source { get; init; }
}
