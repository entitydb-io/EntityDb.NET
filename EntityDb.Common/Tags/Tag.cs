using EntityDb.Abstractions.Tags;

namespace EntityDb.Common.Tags
{
    /// <inheritdoc cref="ITag"/>
    public sealed record Tag(string Scope, string Label, string Value) : ITag
    {
    }
}
