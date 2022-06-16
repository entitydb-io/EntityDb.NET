using EntityDb.Abstractions.Agents;

namespace EntityDb.Common.Agents;

/// <summary>
///     Represents a type that indicates there is no known actor.
/// </summary>
public class UnknownAgentAccessor : IAgentAccessor
{
    /// <inheritdoc/>
    public IAgent GetAgent()
    {
        return new UnknownAgent();
    }
}
