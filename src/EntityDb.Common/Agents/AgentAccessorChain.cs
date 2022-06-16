using EntityDb.Abstractions.Agents;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace EntityDb.Common.Agents;

/// <summary>
///     Represents a type that chains together multiple instances of <see cref="IAgentAccessor" /> and returns the
///     <see cref="IAgent"/> returned by the first <see cref="IAgentAccessor" /> that does not throw an exception.
///
///     If all instances of <see cref="IAgentAccessor"/> throw an exception, this type will throw a
///     <see cref="NoAgentException"/>.
/// </summary>
public class AgentAccessorChain : IAgentAccessor
{
    private readonly ILogger<AgentAccessorChain> _logger;
    private readonly IAgentAccessor[] _agentAccessors;
    
    /// <ignore/>
    public AgentAccessorChain(ILogger<AgentAccessorChain> logger, IOptions<AgentAccessorChainOptions> options) : this(logger, options.Value)
    {
    }
    
    internal AgentAccessorChain(ILogger<AgentAccessorChain> logger, AgentAccessorChainOptions options)
    {
        var serviceProvider = options.ServiceCollection
            .BuildServiceProvider();
        
        _logger = logger;
        _agentAccessors = serviceProvider
            .GetServices<IAgentAccessor>()
            .ToArray();
    }

    /// <inheritdoc />
    public IAgent GetAgent()
    {
        foreach (var agentAccessor in _agentAccessors)
        {
            try
            {
                return agentAccessor.GetAgent();
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "Agent accessor threw an exception. Moving on to next agent accessor");
            }
        }

        throw new NoAgentException();
    }
}
