using System;

namespace EntityDb.Abstractions.ValueObjects;

/// <summary>
///     Represents a unique identifier for an object.
/// </summary>
/// <param name="Value">The backing value.</param>
public readonly record struct Id(Guid Value)
{
    /// <summary>
    ///     Returns a new, randomly-generated <see cref="Id"/>.
    /// </summary>
    /// <returns>A new, randomly-generated <see cref="Id"/>.</returns>
    public static Id NewId() => new(Guid.NewGuid());

    /// <summary>
    ///    Returns a string representation of the value of this instance in
    ///    registry format.
    /// </summary>
    /// <returns>
    ///     The value of this <see cref="Id" />, formatted by using the "D"
    ///     format specifier as follows:  
    ///
    ///     <c>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</c>  
    ///
    ///     where the value of the Id is represented as a series of lowercase
    ///     hexadecimal digits in groups of 8, 4, 4, 4, and 12 digits and
    ///     separated by hyphens. An example of a return value is
    ///     "382c74c3-721d-4f34-80e5-57657b6cbc27". To convert the hexadecimal
    ///     digits from a through f to uppercase, call the
    ///     <see cref="M:System.String.ToUpper" /> method on the returned
    ///     string.
    /// </returns>
    public override string? ToString() => Value.ToString();
}
