using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Common.Sources.Attributes;

/// <inheritdoc cref="ILease" />
public readonly record struct Lease(string Scope, string Label, string Value) : ILease;
