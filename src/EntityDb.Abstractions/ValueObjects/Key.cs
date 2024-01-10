namespace EntityDb.Abstractions.ValueObjects;

/// <summary>
///     Represents a key for a stream.
/// </summary>
/// <param name="Value">The backing value.</param>
public readonly record struct Key(string Value)
{
    /// <inheritdoc cref="string.ToString()" />
    public override string ToString()
    {
        return Value;
    }
}
