namespace EntityDb.Abstractions.States;

/// <summary>
///     References a state with a given id at a given version
/// </summary>
/// <param name="Id">The id of the state.</param>
/// <param name="StateVersion">The version of the state.</param>
public readonly record struct StatePointer(Id Id, StateVersion StateVersion)
{
    /// <summary>
    ///     Prints out <c>{Id}@{Version}</c>.
    ///     See <see cref="Guid.ToString()" /> and
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Id}@{StateVersion}";
    }

    /// <summary>
    ///     Returns the next state pointer.
    /// </summary>
    /// <returns>The next state pointer.</returns>
    public StatePointer Next()
    {
        return Id + StateVersion.Next();
    }
}
