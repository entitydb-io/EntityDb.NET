namespace EntityDb.Abstractions.ValueObjects;

/// <summary>
///     Points to an object (e.g., entity, projection)
/// </summary>
/// <param name="Id">The id of the object.</param>
/// <param name="VersionNumber">The version number of the object.</param>
public record struct Pointer(Id Id, VersionNumber VersionNumber)
{
    /// <summary>
    ///     Checks if the version number found satisfies the pointer.
    /// </summary>
    /// <param name="actualVersionNumber">The actual version number found via queries.</param>
    /// <returns><c>true</c> if </returns>
    public bool IsSatisfiedBy(VersionNumber actualVersionNumber)
    {
        if (VersionNumber == VersionNumber.MinValue)
        {
            return actualVersionNumber != VersionNumber.MinValue;
        }

        return actualVersionNumber == VersionNumber;
    }
}
