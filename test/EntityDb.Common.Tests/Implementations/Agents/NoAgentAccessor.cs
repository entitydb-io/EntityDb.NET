using EntityDb.Abstractions.Agents;

namespace EntityDb.Common.Tests.Implementations.Agents;

public class NoAgentAccessor : IAgentAccessor
{
    public IAgent GetAgent()
    {
        return new NoAgent();
    }
}