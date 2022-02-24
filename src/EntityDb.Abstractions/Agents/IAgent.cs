using System;

namespace EntityDb.Abstractions.Agents;

/// <summary>
///     Represents an actor who can interact with transactions.
/// </summary>
public interface IAgent
{
    /// <summary>
    ///     Returns whether or not the agent has a particular role.
    /// </summary>
    /// <param name="role">The role.</param>
    /// <returns>Whether or not the agent has a particular role.</returns>
    bool HasRole(string role);

    /// <summary>
    ///     Returns the timestamp, as decided by the agent.
    /// </summary>
    /// <returns></returns>
    DateTime GetTimestamp();

    /// <summary>
    ///     Returns an object that represents the signature of the agent.
    /// </summary>
    /// <param name="signatureOptionsName">The name of the signature options configuration.</param>
    /// <returns>An object that represents the signature of the agent.</returns>
    object GetSignature(string signatureOptionsName);
}
