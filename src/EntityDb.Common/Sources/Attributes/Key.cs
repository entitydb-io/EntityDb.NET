using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Common.Sources.Attributes;

/// <summary>
///     Represents a key for a state.
/// </summary>
/// <param name="Value">The backing value.</param>
public readonly record struct Key(string Value) : IStateKey, IMessageKey
{
    private const string KeysScope = "Keys";
    private const string StateLabel = "State";
    private const string MessageLabel = "Message";

    ILease IMessageKey.ToLease(IStateKey streamKey)
    {
        var streamKeyLease = streamKey.ToLease();

        return new Lease($"{KeysScope}/{streamKeyLease.Value}", MessageLabel, Value);
    }

    ILease IStateKey.ToLease()
    {
        return new Lease(KeysScope, StateLabel, Value);
    }

    /// <inheritdoc cref="string.ToString()" />
    public override string ToString()
    {
        return Value;
    }
}
