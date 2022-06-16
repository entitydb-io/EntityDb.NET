using EntityDb.Abstractions.Agents;
using System.Threading;

namespace EntityDb.Common.Agents;

internal abstract class AgentAccessorBase : IAgentAccessor
{
    private readonly AsyncLocal<IAgent?> _agent = new();

    protected abstract IAgent CreateAgent();

    public IAgent GetAgent()
    {
        return _agent.Value ??= CreateAgent();
    }
}
