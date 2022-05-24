using System.Threading;
using System.Threading.Tasks;
using EntityDb.Abstractions.Agents;

namespace EntityDb.Common.Tests.Implementations.Agents;

public class NoAgentAccessor : IAgentAccessor
{
    public async Task<IAgent> GetAgentAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        return new NoAgent();
    }
}