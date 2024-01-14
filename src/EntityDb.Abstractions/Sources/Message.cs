using EntityDb.Abstractions.States.Attributes;
using EntityDb.Abstractions.States.Deltas;
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
    public IReadOnlyCollection<ILease> AddLeases { get; init; } = Array.Empty<ILease>();

    /// <summary>
    ///     The tags to be added.
    /// </summary>
    public IReadOnlyCollection<ITag> AddTags { get; init; } = Array.Empty<ITag>();

    /// <summary>
    ///     The leases to be deleted.
    /// </summary>
    public IReadOnlyCollection<ILease> DeleteLeases { get; init; } = Array.Empty<ILease>();

    /// <summary>
    ///     The tags to be deleted.
    /// </summary>
    public IReadOnlyCollection<ITag> DeleteTags { get; init; } = Array.Empty<ITag>();

    internal static Message NewMessage<TState>
    (
        TState state,
        Pointer statePointer,
        object delta,
        IReadOnlyCollection<ILease>? additionalAddLeases = null
    )
    {
        additionalAddLeases ??= Array.Empty<ILease>();
        
        return new Message
        {
            Id = Id.NewId(),
            StatePointer = statePointer,
            Delta = delta,
            AddLeases = delta is IAddLeasesDelta<TState> addLeasesDelta
                ? addLeasesDelta.GetLeases(state).Concat(additionalAddLeases).ToArray()
                : additionalAddLeases,
            AddTags = delta is IAddTagsDelta<TState> addTagsDelta
                ? addTagsDelta.GetTags(state).ToArray()
                : Array.Empty<ITag>(),
            DeleteLeases = delta is IDeleteLeasesDelta<TState> deleteLeasesDelta
                ? deleteLeasesDelta.GetLeases(state).ToArray()
                : Array.Empty<ILease>(),
            DeleteTags = delta is IDeleteTagsDelta<TState> deleteTagsDelta
                ? deleteTagsDelta.GetTags(state).ToArray()
                : Array.Empty<ITag>(),
        };
    }
}
