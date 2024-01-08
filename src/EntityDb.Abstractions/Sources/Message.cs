using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.ValueObjects;
using System.Collections.Immutable;

namespace EntityDb.Abstractions.Sources;

/// <summary>
///     A message that belongs to a single source
/// </summary>
public sealed record Message
{
    /// <summary>
    ///     A pointer to the entity
    /// </summary>
    public required Pointer EntityPointer { get; init; }

    /// <summary>
    ///     The data.
    /// </summary>
    public required object Delta { get; init; }

    /// <summary>
    ///     The leases to be added.
    /// </summary>
    public ImmutableArray<ILease> AddLeases { get; init; } = ImmutableArray<ILease>.Empty;

    /// <summary>
    ///     The tags to be added.
    /// </summary>
    public ImmutableArray<ITag> AddTags { get; init; } = ImmutableArray<ITag>.Empty;

    /// <summary>
    ///     The tags to be deleted.
    /// </summary>
    public ImmutableArray<ILease> DeleteLeases { get; init; } = ImmutableArray<ILease>.Empty;

    /// <summary>
    ///     The aliases to be added.
    /// </summary>
    public ImmutableArray<ITag> DeleteTags { get; init; } = ImmutableArray<ITag>.Empty;
}
