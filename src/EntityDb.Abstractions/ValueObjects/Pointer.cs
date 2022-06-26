namespace EntityDb.Abstractions.ValueObjects;

/// <summary>
///     Points to an object (e.g., entity, projection)
/// </summary>
/// <param name="Id">The id of the object.</param>
/// <param name="VersionNumber">The version number of the object.</param>
public record struct Pointer(Id Id, VersionNumber VersionNumber);
