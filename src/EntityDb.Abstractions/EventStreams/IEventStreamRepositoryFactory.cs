namespace EntityDb.Abstractions.EventStreams;

/// <summary>
///     Represents a type used to create instances of <see cref="IEventStreamRepository" />.
/// </summary>
public interface IEventStreamRepositoryFactory
{
    /// <summary>
    ///     Creates a new instance of <see cref="IEventStreamRepository" />.
    /// </summary>
    /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
    /// <param name="sourceSessionOptionsName">The name of the source session options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="IEventStreamRepository" />.</returns>
    Task<IEventStreamRepository> CreateRepository(string agentSignatureOptionsName,
        string sourceSessionOptionsName, CancellationToken cancellationToken = default);
}
