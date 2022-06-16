using EntityDb.Abstractions.Agents;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Agents;

/// <summary>
///     Options for configuring the <see cref="AgentAccessorChain"/>.
/// </summary>
public sealed class AgentAccessorChainOptions
{
    /// <summary>
    ///     A service collection used to resolve multiple instances of <see cref="IAgentAccessor"/>.
    /// </summary>
    public IServiceCollection ServiceCollection { get; } = new ServiceCollection();
}
