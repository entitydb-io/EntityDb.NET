using EntityDb.Abstractions.Sources;

namespace EntityDb.Common.Sources.Processors.Queues;

/// <summary>
///     An item of work for a <see cref="ISourceProcessorQueue" />
/// </summary>
public interface ISourceProcessorQueueItem
{
    /// <summary>
    ///     The type of the source processor, which *must* implement
    ///     <see cref="ISourceProcessor" />.
    /// </summary>
    Type SourceProcessorType { get; }

    /// <summary>
    ///     The source to be processed.
    /// </summary>
    Source Source { get; }
}
