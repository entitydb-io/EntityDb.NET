namespace EntityDb.Abstractions.Tags
{
    /// <summary>
    /// Represents a single metadata property, which can be used to query the current state of an entity.
    /// </summary>
    public interface ITag
    {
        /// <summary>
        /// The name of the metadata property.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// The value of the metadata property.
        /// </summary>
        string Value { get; }
    }
}
