using EntityDb.Abstractions.Agents;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Agents;

internal abstract class AgentAccessorBase : IAgentAccessor
{
    private IAgent? _agent;

    protected abstract Task<IAgent> CreateAgentAsync(CancellationToken cancellationToken);

    public async Task<IAgent> GetAgentAsync(CancellationToken cancellationToken = default)
    {
        return _agent ??= await CreateAgentAsync(cancellationToken);
    }
}
