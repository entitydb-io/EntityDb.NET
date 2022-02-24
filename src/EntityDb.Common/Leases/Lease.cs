using EntityDb.Abstractions.Leases;

namespace EntityDb.Common.Leases;

/// <inheritdoc cref="ILease" />
public sealed record Lease(string Scope, string Label, string Value) : ILease;
