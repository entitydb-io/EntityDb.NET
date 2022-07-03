using System;

namespace EntityDb.Abstractions.ValueObjects;

/// <summary>
///     Represents a unique identifier for an object.
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
        return new(Guid.NewGuid());
    }

    /// <summary>
    ///     Returns a string representation of the value of this instance in
    ///     registry format.
    /// </summary>
    /// <returns>
    ///     The value of this <see cref="Id" />, formatted by using the "D"
    ///     format specifier as follows:
    ///     <c>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</c>
    ///     where the value of the Id is represented as a series of lowercase
    ///     hexadecimal digits in groups of 8, 4, 4, 4, and 12 digits and
    ///     separated by hyphens. An example of a return value is
    ///     "382c74c3-721d-4f34-80e5-57657b6cbc27". To convert the hexadecimal
    ///     digits from a through f to uppercase, call the
    ///     <see cref="M:System.String.ToUpper" /> method on the returned
    ///     string.
    /// </returns>
    public override string? ToString()
    {
        return Value.ToString();
    }

    /// <summary>
    ///     Implicitly converts an <see cref="Id" /> to a pointer.
    /// </summary>
    /// <param name="id">The implicit id argument.</param>
    public static implicit operator Pointer(Id id)
    {
        return id + VersionNumber.MinValue;
    }

    /// <summary>
    ///     Combine an Id and a VersionNumber into a Pointer.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="versionNumber"></param>
    /// <returns>A pointer for the id and version number.</returns>
    public static Pointer operator +(Id id, VersionNumber versionNumber)
    {
        return new Pointer(id, versionNumber);
    }
}
