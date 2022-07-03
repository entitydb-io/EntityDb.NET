using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Agents;

/// <summary>
///     Represents an actor who can interact with transactions.
/// </summary>
public interface IAgent
{
    /// <summary>
    ///     Gets the current timestamp, according to the agent.
    /// </summary>
    TimeStamp TimeStamp { get; }

    /// <summary>
    ///     Gets the signature of the agent, can can be serialized to a database.
    /// </summary>
    object Signature { get; }
}
