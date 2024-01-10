using EntityDb.Abstractions.States.Attributes;

namespace EntityDb.Common.States.Attributes;

/// <inheritdoc cref="ITag" />
public sealed record Tag(string Label, string Value) : ITag;
