using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Agents;

/// <summary>
///     Represents an actor who can interact with transactions.
/// </summary>
public interface IAgent
{
    TimeStamp TimeStamp { get; }

    object Signature { get; }
}
