using EntityDb.Abstractions.Agents;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;
using System.Threading.Tasks;
using System.Threading;

namespace EntityDb.Common.Tests.Agents;

public abstract class AgentAccessorTestsBase<TStartup, TAgentAccessorConfiguration> : TestsBase<TStartup>
    where TStartup : IStartup, new()
{
    protected AgentAccessorTestsBase(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    protected abstract void ConfigureInactiveAgentAccessor(IServiceCollection serviceCollection);

    protected abstract void ConfigureActiveAgentAccessor(IServiceCollection serviceCollection, TAgentAccessorConfiguration agentAccessorConfiguration);

    protected abstract Dictionary<string, string>? GetApplicationInfo(object agentSignature);

    protected abstract IEnumerable<TAgentAccessorConfiguration> GetAgentAccessorOptions();

    [Fact]
    public async Task GivenBackingServiceInactive_WhenGettingAgent_ThenThrow()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(ConfigureInactiveAgentAccessor);

        var agentAccessor = serviceScope.ServiceProvider
            .GetRequiredService<IAgentAccessor>();

        // ASSERT

        await Should.ThrowAsync<NoAgentException>(() => agentAccessor.GetAgentAsync());
    }

    [Fact]
    public async Task GivenBackingServiceActive_WhenGettingAgent_ThenReturnAgent()
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

            var agent = await agentAccessor.GetAgentAsync();

            // ASSERT

            agent.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task GivenBackingServiceActive_ThenCanGetTimestamp()
    {
        foreach (var agentAccessorConfiguration in GetAgentAccessorOptions())
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, agentAccessorConfiguration);
            });

            // ACT

            var agent = await serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>()
                .GetAgentAsync();

            // ASSERT

            Should.NotThrow(() => agent.GetTimeStamp());
        }
    }

    [Fact]
    public async Task GivenBackingServiceActive_WhenGettingAgentSignature_ThenReturnAgentSignature()
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

            var agentSignature = (await agentAccessor
                .GetAgentAsync())
                .GetSignature("");

            // ASSERT

            agentSignature.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task GivenBackingServiceActiveAndNoSignatureAugmenter_WhenGettingApplicationInfo_ThenReturnEmptyApplicationInfo()
    {
        foreach (var agentAccessorConfiguration in GetAgentAccessorOptions())
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, agentAccessorConfiguration);

                serviceCollection.RemoveAll(typeof(IAgentSignatureAugmenter));
            });

            var agentAccessor = serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>();

            // ACT

            var agentSignature = (await agentAccessor
                .GetAgentAsync())
                .GetSignature("")
                .ShouldNotBeNull();

            var applicationInfo = GetApplicationInfo(agentSignature);

            // ASSERT

            applicationInfo.ShouldBeEmpty();
        }
    }
    

    [Fact]
    public async Task GivenBackingServiceActiveAndHasSignatureAugmenter_WhenGettingApplicationInfo_ThenReturnExpectedApplicationInfo()
    {
        foreach (var agentAccessorConfiguration in GetAgentAccessorOptions())
        {
            // ARRANGE

            var expectedApplicationInfo = new Dictionary<string, string>
            {
                ["UserId"] = Guid.NewGuid().ToString()
            };

            var agentSignatureAugmenterMock = new Mock<IAgentSignatureAugmenter>(MockBehavior.Strict);

            agentSignatureAugmenterMock
                .Setup(x => x.GetApplicationInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedApplicationInfo);

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, agentAccessorConfiguration);

                serviceCollection.AddSingleton(agentSignatureAugmenterMock.Object);
            });

            var agentAccessor = serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>();

            // ACT

            var agentSignature = (await agentAccessor
                .GetAgentAsync())
                .GetSignature("");

            var actualApplicationInfo = GetApplicationInfo(agentSignature);

            // ASSERT

            actualApplicationInfo.ShouldBe(expectedApplicationInfo);
        }
    }
}