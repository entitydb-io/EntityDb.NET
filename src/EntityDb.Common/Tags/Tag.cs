using EntityDb.Abstractions.Tags;

namespace EntityDb.Common.Tags
{
    /// <inheritdoc cref="ITag" />
    public sealed record Tag(string Label, string Value) : ITag
    {
    }
}
