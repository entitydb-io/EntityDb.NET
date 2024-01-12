using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Sources.Agents;

public abstract class AgentAccessorTestsBase<TStartup, TAgentAccessorConfiguration> : TestsBase<TStartup>
    where TStartup : IStartup, new()
{
    protected AgentAccessorTestsBase(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    protected abstract bool CanBeInactive { get; }

    protected abstract void ConfigureInactiveAgentAccessor(IServiceCollection serviceCollection);

    protected abstract void ConfigureActiveAgentAccessor(IServiceCollection serviceCollection,
        TAgentAccessorConfiguration agentAccessorConfiguration);

    protected abstract Dictionary<string, string>? GetApplicationInfo(object agentSignature);

    protected abstract IEnumerable<TAgentAccessorConfiguration> GetAgentAccessorOptions();

    [Fact]
    public async Task GivenBackingServiceInactive_WhenGettingAgent_ThenThrow()
    {
        if (!CanBeInactive)
        {
            return;
        }

        // ARRANGE

        await using var serviceScope = CreateServiceScope(ConfigureInactiveAgentAccessor);

        var agentAccessor = serviceScope.ServiceProvider
            .GetRequiredService<IAgentAccessor>();

        // ASSERT

        Should.Throw<NoAgentException>(() => agentAccessor.GetAgent(default!));
    }

    [Fact]
    public async Task GivenBackingServiceActive_WhenGettingAgent_ThenReturnAgent()
    {
        foreach (var agentAccessorConfiguration in GetAgentAccessorOptions())
        {
            // ARRANGE

            await using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, agentAccessorConfiguration);
            });

            var agentAccessor = serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>();

            // ACT

            var agent = await agentAccessor.GetAgent(default!);

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

            await using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, agentAccessorConfiguration);
            });

            // ACT

            var agent = await serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>()
                .GetAgent(default!);

            // ASSERT

            Should.NotThrow(() => agent.TimeStamp);
        }
    }

    [Fact]
    public async Task GivenBackingServiceActive_WhenGettingAgentSignature_ThenReturnAgentSignature()
    {
        foreach (var agentAccessorConfiguration in GetAgentAccessorOptions())
        {
            // ARRANGE

            await using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, agentAccessorConfiguration);
            });

            var agent = await serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>()
                .GetAgent(default!);

            // ACT

            var agentSignature = agent.Signature;

            // ASSERT

            agentSignature.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task
        GivenBackingServiceActiveAndNoSignatureAugmenter_WhenGettingApplicationInfo_ThenReturnEmptyApplicationInfo()
    {
        foreach (var agentAccessorConfiguration in GetAgentAccessorOptions())
        {
            // ARRANGE

            await using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, agentAccessorConfiguration);

                serviceCollection.RemoveAll(typeof(IAgentSignatureAugmenter));
            });

            var agent = await serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>()
                .GetAgent(default!);

            // ACT

            var agentSignature = agent.Signature;

            var applicationInfo = GetApplicationInfo(agentSignature);

            // ASSERT

            applicationInfo.ShouldBeEmpty();
        }
    }


    [Fact]
    public async Task
        GivenBackingServiceActiveAndHasSignatureAugmenter_WhenGettingApplicationInfo_ThenReturnExpectedApplicationInfo()
    {
        foreach (var agentAccessorConfiguration in GetAgentAccessorOptions())
        {
            // ARRANGE

            var expectedApplicationInfo = new Dictionary<string, string> { ["UserId"] = Guid.NewGuid().ToString() };

            var agentSignatureAugmenterMock = new Mock<IAgentSignatureAugmenter>(MockBehavior.Strict);

            agentSignatureAugmenterMock
                .Setup(x => x.GetApplicationInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedApplicationInfo);

            await using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, agentAccessorConfiguration);

                serviceCollection.AddSingleton(agentSignatureAugmenterMock.Object);
            });

            var agent = await serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>()
                .GetAgent(default!);

            // ACT

            var agentSignature = agent.Signature;

            var actualApplicationInfo = GetApplicationInfo(agentSignature);

            // ASSERT

            actualApplicationInfo.ShouldBe(expectedApplicationInfo);
        }
    }
}
