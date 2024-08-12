using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Common.Sources.Attributes;

/// <inheritdoc cref="ITag" />
public readonly record struct Tag(string Label, string Value) : ITag;
