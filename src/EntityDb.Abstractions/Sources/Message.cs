using EntityDb.Abstractions.Extensions;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.States;
using EntityDb.Abstractions.States.Deltas;

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
    ///     The state pointer
    /// </summary>
    public required StatePointer StatePointer { get; init; }

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

    internal static Message NewMessage<TState>
    (
        TState state,
        StatePointer statePointer,
        object delta,
        IStateKey? stateKey = default
    )
    {
        IEnumerable<ILease>? addLeases = default;

        if (stateKey != default)
        {
            if (statePointer.StateVersion == StateVersion.One)
            {
                addLeases = new[] { stateKey.ToLease() };
            }

            if (delta is IAddAlternateStateKeysDelta<TState> addAlternateStateKeysDelta)
            {
                addLeases = addLeases
                    .ConcatOrCoalesce(addAlternateStateKeysDelta
                        .GetAlternateStateKeys(state)
                        .Select(key => key.ToLease()));
            }

            if (delta is IAddMessageKeyDelta addMessageKeyDelta)
            {
                addLeases = addLeases
                    .AppendOrStart(addMessageKeyDelta
                        .GetMessageKey()
                        .ToLease(stateKey));
            }
        }
        else if (delta is IAddLeasesDelta<TState> addLeasesDelta)
        {
            addLeases = addLeasesDelta.GetLeases(state);
        }

        return new Message
        {
            Id = Id.NewId(),
            StatePointer = statePointer,
            Delta = delta,
            AddLeases = addLeases?.ToArray() ?? Array.Empty<ILease>(),
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
