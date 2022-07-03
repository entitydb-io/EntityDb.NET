using EntityDb.Abstractions.Agents;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Agents;

/// <summary>
///     Represents a type that chains together multiple instances of <see cref="IAgentAccessor" /> and returns the
///     <see cref="IAgent" /> returned by the first <see cref="IAgentAccessor" /> that does not throw an exception.
///     If all instances of <see cref="IAgentAccessor" /> throw an exception, this type will throw a
///     <see cref="NoAgentException" />.
/// </summary>
public class AgentAccessorChain : IAgentAccessor
{
    private readonly IAgentAccessor[] _agentAccessors;
    private readonly ILogger<AgentAccessorChain> _logger;

    /// <ignore />
    public AgentAccessorChain
    (
        ILogger<AgentAccessorChain> logger,
        IOptions<AgentAccessorChainOptions> options,
        IServiceProvider outerServiceProvider
    ) : this(logger, options.Value, outerServiceProvider)
    {
    }

    internal AgentAccessorChain
    (
        ILogger<AgentAccessorChain> logger,
        AgentAccessorChainOptions options,
        IServiceProvider outerServiceProvider
    )
    {
        var serviceProvider = GetServiceProvider(outerServiceProvider, options);

        _logger = logger;
        _agentAccessors = serviceProvider
            .GetServices<IAgentAccessor>()
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<IAgent> GetAgentAsync(string signatureOptionsName, CancellationToken cancellationToken)
    {
        foreach (var agentAccessor in _agentAccessors)
        {
            try
            {
                return await agentAccessor.GetAgentAsync(signatureOptionsName, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "Agent accessor threw an exception. Moving on to next agent accessor");
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        throw new NoAgentException();
    }

    private static IServiceProvider GetServiceProvider(IServiceProvider outerServiceProvider,
        AgentAccessorChainOptions options)
    {
        IServiceCollection serviceCollectionCopy = new ServiceCollection();

        foreach (var (outerServiceType, innerServiceLifetime) in options.RequiredOuterServices)
        {
            serviceCollectionCopy.Add(new ServiceDescriptor
            (
                outerServiceType,
                _ => outerServiceProvider.GetRequiredService(outerServiceType),
                innerServiceLifetime
            ));
        }

        foreach (var serviceDescriptor in options.ServiceCollection)
        {
            serviceCollectionCopy.Add(serviceDescriptor);
        }

        return serviceCollectionCopy.BuildServiceProvider();
    }
}
