namespace EntityDb.Abstractions.States.Attributes;

/// <summary>
///     Represents a single metadata property.
/// </summary>
public interface ITag
{
    /// <summary>
    ///     The name of the metadata property.
    /// </summary>
    string Label { get; }

    /// <summary>
    ///     The value of the metadata property.
    /// </summary>
    string Value { get; }
}
