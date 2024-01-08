using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Common.Sources.Attributes;

/// <inheritdoc cref="ITag" />
public sealed record Tag(string Label, string Value) : ITag;
