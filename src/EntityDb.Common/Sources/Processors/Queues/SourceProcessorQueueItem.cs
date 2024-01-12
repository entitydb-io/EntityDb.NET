using EntityDb.Abstractions.Sources;

namespace EntityDb.Common.Sources.Processors.Queues;

internal sealed class SourceProcessorQueueItem : ISourceProcessorQueueItem
{
    public required Type SourceProcessorType { get; init; }
    public required Source Source { get; init; }
}
