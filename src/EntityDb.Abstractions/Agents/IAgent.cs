using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Agents;

/// <summary>
///     Represents an actor who can interact with transactions.
/// </summary>
public interface IAgent
{
    /// <summary>
    ///     Returns the timestamp, as decided by the agent.
    /// </summary>
    /// <returns></returns>
    TimeStamp GetTimeStamp();

    /// <summary>
    ///     Returns an object that represents the signature of the agent.
    /// </summary>
    /// <param name="signatureOptionsName">The name of the signature options configuration.</param>
    /// <returns>An object that represents the signature of the agent.</returns>
    object GetSignature(string signatureOptionsName);
}
