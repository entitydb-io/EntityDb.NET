using EntityDb.Abstractions.Disposables;

namespace EntityDb.Abstractions.States;

/// <summary>
///     Represents a type used to create instances of <see cref="IStateRepository{TState}" />
/// </summary>
/// <typeparam name="TState">The type of state stored by the <see cref="IStateRepository{TState}" />.</typeparam>
public interface IStateRepositoryFactory<TState> : IDisposableResource
{
    /// <summary>
    ///     Create a new instance of <see cref="IStateRepository{TState}" />
    /// </summary>
    /// <param name="stateSessionOptionsName">The agent's use case for the repository.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="IStateRepository{TState}" />.</returns>
    Task<IStateRepository<TState>> Create(string stateSessionOptionsName,
        CancellationToken cancellationToken = default);
}
