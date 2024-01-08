using EntityDb.Abstractions.ValueObjects;
using System.Collections.Immutable;

namespace EntityDb.Abstractions.Sources;

/// <summary>
///     Represents a set of messages that can be committed.
/// </summary>
public sealed record Source
{
    /// <summary>
    ///     The id assigned to the source.
    /// </summary>
    public required Id Id { get; init; }

    /// <summary>
    ///     The date and time associated with the source, according to the agent.
    /// </summary>
    public required TimeStamp TimeStamp { get; init; }

    /// <summary>
    ///     The signature of the agent.
    /// </summary>
    public required object AgentSignature { get; init; }

    /// <summary>
    ///     The messages of the source.
    /// </summary>
    public required ImmutableArray<Message> Messages { get; init; }
}
