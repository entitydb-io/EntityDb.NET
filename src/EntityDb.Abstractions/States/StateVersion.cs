namespace EntityDb.Abstractions.States;

/// <summary>
///     Represents a version for an state.
/// </summary>
/// <param name="Value">The backing value.</param>
public readonly record struct StateVersion(ulong Value)
{
    /// <summary>
    ///     In the context of a <see cref="StatePointer" /> this
    ///     refers to the latest state. In the context of a state
    ///     itself, this refers to the initial state.
    /// </summary>
    public static readonly StateVersion Zero = new(ulong.MinValue);

    /// <summary>
    ///     This always refers to the first version of a state.
    /// </summary>
    public static readonly StateVersion One = new(1);

    /// <summary>
    ///     Returns the next version.
    /// </summary>
    /// <returns>The next version.</returns>
    public StateVersion Previous()
    {
        return new StateVersion(Value - 1);
    }

    /// <summary>
    ///     Returns the next version.
    /// </summary>
    /// <returns>The next version.</returns>
    public StateVersion Next()
    {
        return new StateVersion(Value + 1);
    }

    /// <inheritdoc cref="ulong.ToString()" />
    public override string ToString()
    {
        return Value.ToString();
    }
}
