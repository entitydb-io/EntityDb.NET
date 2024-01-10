using EntityDb.Abstractions.States.Attributes;

namespace EntityDb.Common.States.Attributes;

/// <inheritdoc cref="ILease" />
public sealed record Lease(string Scope, string Label, string Value) : ILease;
