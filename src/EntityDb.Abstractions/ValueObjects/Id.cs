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
}
