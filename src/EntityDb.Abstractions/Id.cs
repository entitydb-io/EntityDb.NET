using EntityDb.Abstractions.States;

namespace EntityDb.Abstractions;

/// <summary>
///     Represents an identifier for a sources, messages, and states.
/// </summary>
/// <param name="Value">The backing value.</param>
public readonly record struct Id(Guid Value)
{
    /// <summary>
    ///     Returns a new, randomly-generated <see cref="Id" />.
    /// </summary>
    /// <returns>A new, randomly-generated <see cref="Id" />.</returns>
    public static Id NewId()
    {
        return new Id(Guid.NewGuid());
    }

    /// <inheritdoc cref="Guid.ToString()" />
    public override string ToString()
    {
        return Value.ToString();
    }

    /// <summary>
    ///     Implicitly converts a state id into a state pointer.
    /// </summary>
    /// <param name="stateId">The implicit state id argument.</param>
    public static implicit operator StatePointer(Id stateId)
    {
        return stateId + StateVersion.Zero;
    }

    /// <summary>
    ///     Combine a state id and a state version into a state pointer.
    /// </summary>
    /// <param name="stateId">The state id.</param>
    /// <param name="stateVersion">The state version</param>
    /// <returns>A state pointer.</returns>
    public static StatePointer operator +(Id stateId, StateVersion stateVersion)
    {
        return new StatePointer(stateId, stateVersion);
    }
}
