using EntityDb.Abstractions.Agents;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace EntityDb.Common.Tests.Agents;

public abstract class AgentAccessorTestsBase<TStartup, TAgentAccessorConfiguration> : TestsBase<TStartup>
    where TStartup : IStartup, new()
{
    protected AgentAccessorTestsBase(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    protected abstract void ConfigureInactiveAgentAccessor(IServiceCollection serviceCollection);

    protected abstract void ConfigureActiveAgentAccessor(IServiceCollection serviceCollection, TAgentAccessorConfiguration agentAccessorConfiguration);

    protected abstract IEnumerable<TAgentAccessorConfiguration> GetAgentAccessorOptions();

    [Fact]
    public void GivenBackingServiceInactive_WhenGettingAgent_ThenThrow()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(ConfigureInactiveAgentAccessor);

        var agentAccessor = serviceScope.ServiceProvider
            .GetRequiredService<IAgentAccessor>();

        // ASSERT

        Should.Throw<NoAgentException>(() => agentAccessor.GetAgent());
    }

    [Fact]
    public void GivenBackingServiceActive_WhenGettingAgent_ThenReturnAgent()
    {
        foreach (var agentAccessorConfiguration in GetAgentAccessorOptions())
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, agentAccessorConfiguration);
            });

            var agentAccessor = serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>();

            // ACT

            var agent = agentAccessor.GetAgent();

            // ASSERT

            agent.ShouldNotBeNull();
        }
    }

    [Fact]
    public void GivenBackingServiceActive_ThenCanGetTimestamp()
    {
        foreach (var agentAccessorConfiguration in GetAgentAccessorOptions())
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, agentAccessorConfiguration);
            });

            // ACT

            var agent = serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>()
                .GetAgent();

            // ASSERT

            Should.NotThrow(() => agent.GetTimeStamp());
        }
    }

    [Fact]
    public void GivenBackingServiceActive_WhenGettingAgentSignature_ThenReturnAgentSignature()
    {
        foreach (var agentAccessorConfiguration in GetAgentAccessorOptions())
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, agentAccessorConfiguration);
            });

            var agentAccessor = serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>();

            // ACT

            var agentSignature = agentAccessor.GetAgent().GetSignature("");

            // ASSERT

            agentSignature.ShouldNotBeNull();
        }
    }
}