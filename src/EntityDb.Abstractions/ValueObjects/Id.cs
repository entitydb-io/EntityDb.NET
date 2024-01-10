namespace EntityDb.Abstractions.ValueObjects;

/// <summary>
///     Represents an identifier for a state.
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
        return new Id(Guid.NewGuid());
    }

    /// <inheritdoc cref="Guid.ToString()" />
    public override string ToString()
    {
        return Value.ToString();
    }

    /// <summary>
    ///     Implicitly converts an <see cref="Id" /> to a pointer.
    /// </summary>
    /// <param name="id">The implicit id argument.</param>
    public static implicit operator Pointer(Id id)
    {
        return id + Version.Zero;
    }

    /// <summary>
    ///     Combine an Id and a Version into a Pointer.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    /// <returns>A pointer for the id and version.</returns>
    public static Pointer operator +(Id id, Version version)
    {
        return new Pointer(id, version);
    }
}
