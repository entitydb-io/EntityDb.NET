namespace EntityDb.Abstractions.Agents;

/// <summary>
///     Represents a type that can access an instance of <see cref="IAgent" />.
/// </summary>
public interface IAgentAccessor
{
    /// <summary>
    ///     Returns the agent.
    /// </summary>
    /// <returns>The agent.</returns>
    IAgent GetAgent();
}
