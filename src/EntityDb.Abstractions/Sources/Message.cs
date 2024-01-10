using EntityDb.Abstractions.States.Attributes;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Sources;

/// <summary>
///     A message that belongs to a single source
/// </summary>
public sealed record Message
{
    /// <summary>
    ///     The id assigned to the message.
    /// </summary>
    public required Id Id { get; init; }

    /// <summary>
    ///     A pointer to the state
    /// </summary>
    public required Pointer StatePointer { get; init; }

    /// <summary>
    ///     The data.
    /// </summary>
    public required object Delta { get; init; }

    /// <summary>
    ///     The leases to be added.
    /// </summary>
    public ILease[] AddLeases { get; init; } = Array.Empty<ILease>();

    /// <summary>
    ///     The tags to be added.
    /// </summary>
    public ITag[] AddTags { get; init; } = Array.Empty<ITag>();

    /// <summary>
    ///     The leases to be deleted.
    /// </summary>
    public ILease[] DeleteLeases { get; init; } = Array.Empty<ILease>();

    /// <summary>
    ///     The tags to be deleted.
    /// </summary>
    public ITag[] DeleteTags { get; init; } = Array.Empty<ITag>();
}
