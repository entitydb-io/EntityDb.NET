using EntityDb.Abstractions.Agents;

namespace EntityDb.Common.Agents;

internal abstract class AgentAccessorBase : IAgentAccessor
{
    private IAgent? _agent;

    protected abstract IAgent CreateAgent();

    public IAgent GetAgent()
    {
        return _agent ??= CreateAgent();
    }
}
