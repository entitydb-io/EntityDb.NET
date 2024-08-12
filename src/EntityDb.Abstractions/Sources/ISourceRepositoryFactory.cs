using EntityDb.Abstractions.Disposables;

namespace EntityDb.Abstractions.Sources;

/// <summary>
///     Represents a type used to create instances of <see cref="ISourceRepository" />.
/// </summary>
public interface ISourceRepositoryFactory : IDisposableResource
{
    /// <summary>
    ///     Creates a new instance of <see cref="ISourceRepository" />.
    /// </summary>
    /// <param name="sourceSessionOptionsName">The agent's use case for the repository.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="ISourceRepository" />.</returns>
    Task<ISourceRepository> Create(string sourceSessionOptionsName,
        CancellationToken cancellationToken = default);
}
