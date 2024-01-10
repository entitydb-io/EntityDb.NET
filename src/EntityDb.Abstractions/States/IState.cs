using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.States;

/// <summary>
///     Indicates that the state is compatible with several EntityDb.Common implementations.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public interface IState<TState>
{
    /// <summary>
    ///     Creates a new instance of a <typeparamref name="TState" />.
    /// </summary>
    /// <param name="pointer">The pointer of the state.</param>
    /// <returns>A new instance of <typeparamref name="TState" />.</returns>
    static abstract TState Construct(Pointer pointer);

    /// <summary>
    ///     Returns a pointer for the current state of the state.
    /// </summary>
    /// <returns>A pointer for the current state of the state</returns>
    Pointer GetPointer();

    /// <summary>
    ///     Indicates if this state instance version should be recorded (independent of the latest state).
    /// </summary>
    /// <returns><c>true</c> if this state instance should be recorded, or else <c>false</c>.</returns>
    /// <remarks>
    ///     You would use this if you intent to fetch a state at multiple versions and don't want to hit
    ///     the source database when it can be avoided.
    /// </remarks>
    bool ShouldRecord();

    /// <summary>
    ///     Indicates if this state instance should be recorded as the latest state.
    /// </summary>
    /// <param name="previousLatestState">The previous instance of the latest state.</param>
    /// <returns><c>true</c> if this state instance should be recorded as the latest state, or else <c>false</c>.</returns>
    bool ShouldRecordAsLatest(TState? previousLatestState);
}
