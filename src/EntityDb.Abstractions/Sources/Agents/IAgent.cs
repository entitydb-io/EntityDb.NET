namespace EntityDb.Abstractions.Sources.Agents;

/// <summary>
///     Represents an actor who can record sources.
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
