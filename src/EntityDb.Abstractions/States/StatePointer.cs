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
    ///     Checks if the state pointer found satisfies the state pointer requested.
    /// </summary>
    /// <param name="candidateStatePointer">A candidate state pointer.</param>
    public bool IsSatisfiedBy(StatePointer candidateStatePointer)
    {
        if (Id != candidateStatePointer.Id)
        {
            return false;
        }
        
        if (StateVersion == StateVersion.Zero)
        {
            return candidateStatePointer.StateVersion != StateVersion.Zero;
        }

        return StateVersion == candidateStatePointer.StateVersion;
    }

    /// <summary>
    ///     Returns the next state pointer.
    /// </summary>
    /// <returns>The next state pointer.</returns>
    public StatePointer Previous()
    {
        return Id + StateVersion.Previous();
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
