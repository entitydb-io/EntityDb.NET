using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.States;

/// <summary>
///     Represents a collection of <typeparamref name="TState" /> states.
/// </summary>
/// <typeparam name="TState">The type of state stored in the <see cref="IStateRepository{TState}" />.</typeparam>
public interface IStateRepository<TState> : IDisposableResource
{
    /// <summary>
    ///     Returns an exact version of state of a <typeparamref name="TState" /> or
    ///     <c>default(<typeparamref name="TState" />)</c>.
    /// </summary>
    /// <param name="statePointer">A pointer to a specific state.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     An exact version of state of a <typeparamref name="TState" /> or
    ///     <c>default(<typeparamref name="TState" />)</c>.
    /// </returns>
    Task<TState?> Get(Pointer statePointer, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Inserts a <typeparamref name="TState" /> state.
    /// </summary>
    /// <param name="statePointer">A pointer to a state.</param>
    /// <param name="state">The state.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the insert succeeded, or <c>false</c> if the insert failed.</returns>
    Task<bool> Put(Pointer statePointer, TState state, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes multiple <typeparamref name="TState" /> states.
    /// </summary>
    /// <param name="statePointers">Pointers to specific states.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the deletes all succeeded, or <c>false</c> if any deletes failed.</returns>
    Task<bool> Delete(Pointer[] statePointers, CancellationToken cancellationToken = default);
}
