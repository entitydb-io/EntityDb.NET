namespace EntityDb.Abstractions.ValueObjects;

/// <summary>
///     Represents a particular version for an object.
/// </summary>
/// <param name="Value">The backing value.</param>
public readonly record struct VersionNumber(ulong Value)
{
    /// <summary>
    ///     This constant represents the minimum version number, which is typically reserved for the initial state of an object.
    /// </summary>
    public static readonly VersionNumber MinValue = new(ulong.MinValue);
    
    /// <summary>
    ///     Gets the next version number.
    /// </summary>
    /// <returns>The next version number.</returns>
    public VersionNumber Next() => new(Value + 1);
}
