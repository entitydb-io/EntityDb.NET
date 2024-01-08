namespace EntityDb.Abstractions.ValueObjects;

/// <summary>
///     Points to an entity or projection
/// </summary>
/// <param name="Id">The id of the entity or projection.</param>
/// <param name="Version">The version of the entity or projection.</param>
public readonly record struct Pointer(Id Id, Version Version)
{
    /// <summary>
    ///     Prints out <c>{Id}@{Version}</c>.
    ///     See <see cref="Guid.ToString()" /> and
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Id}@{Version}";
    }

    /// <summary>
    ///     Checks if the pointer found satisfies the pointer.
    /// </summary>
    /// <param name="actualPointer">The actual version found via queries.</param>
    /// <returns><c>true</c> if </returns>
    public bool IsSatisfiedBy(Pointer actualPointer)
    {
        if (Id != actualPointer.Id)
        {
            return false;
        }

        if (Version == Version.Zero)
        {
            return actualPointer.Version != Version.Zero;
        }

        return actualPointer.Version == Version;
    }

    /// <summary>
    ///     Returns the next pointer.
    /// </summary>
    /// <returns>The next pointer.</returns>
    public Pointer Next()
    {
        return Id + Version.Next();
    }
}
