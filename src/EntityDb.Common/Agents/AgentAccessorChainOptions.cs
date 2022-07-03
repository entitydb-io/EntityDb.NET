using EntityDb.Abstractions.Agents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace EntityDb.Common.Agents;

/// <summary>
///     Options for configuring the <see cref="AgentAccessorChain" />.
/// </summary>
public sealed class AgentAccessorChainOptions
{
    /// <summary>
    ///     An inner service collection used to configure and resolve multiple instances of <see cref="IAgentAccessor" />.
    /// </summary>
    public IServiceCollection ServiceCollection { get; } = new ServiceCollection();

    /// <summary>
    ///     If any of the instances of <see cref="IAgentAccessor" /> which are added to <see cref="ServiceCollection" />
    ///     have required dependencies that live in the outer service provider (the one constructing
    ///     <see cref="AgentAccessorChainOptions" />),
    ///     then you can specify them here.
    /// </summary>
    /// <remarks>
    ///     Note that the <see cref="ServiceLifetime.Scoped">ServiceLifetime.Scoped</see> is not supported,
    ///     and if the outer service is itself scoped, the agent accessor will not be able to receive it
    ///     because it is, itself, registered as a singleton service.
    /// </remarks>
    public Dictionary<Type, ServiceLifetime> RequiredOuterServices { get; } = new();
}
