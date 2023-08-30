using EntityDb.Abstractions.Sources;

namespace EntityDb.Common.Sources.Processors;

/// <summary>
///     Represents a type that processes sources emitted to a <see cref="ISourceSubscriber"/>.
/// </summary>
public interface ISourceProcessor
{
    /// <summary>
    ///     Defines the procedure for processing a given source.
    /// </summary>
    /// <param name="source">The source that has been received.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task</returns>
    Task Process(ISource source, CancellationToken cancellationToken);
}
