using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Common.Sources.Attributes;

/// <inheritdoc cref="ILease" />
public sealed record Lease(string Scope, string Label, string Value) : ILease;
