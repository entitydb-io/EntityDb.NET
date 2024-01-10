using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Common.Sources.Processors;

namespace EntityDb.Common.Sources.ReprocessorQueues;

/// <summary>
///     Represents a request for a <see cref="ISourceProcessor" /> to reprocess sources.
/// </summary>
public interface ISourceReprocessorQueueItem
{
    /// <summary>
    ///     The name of the source session options passed to
    ///     <see cref="ISourceRepositoryFactory.Create" />
    /// </summary>
    string SourceSessionOptionsName { get; }

    /// <summary>
    ///     The type of the source processor, which *must*
    ///     implement <see cref="ISourceProcessor" />.
    /// </summary>
    Type SourceProcessorType { get; }

    /// <summary>
    ///     Determines which sources need to be reprocessed.
    /// </summary>
    IQuery Query { get; }

    /// <summary>
    ///     Determines how long to wait between each call to enqueue.
    /// </summary>
    TimeSpan EnqueueDelay { get; }
}
