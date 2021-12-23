using EntityDb.Abstractions.Agents;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using Xunit;

namespace EntityDb.Common.Tests.Agents
{
    public abstract class AgentAccessorTestsBase<TStartup, TAgentAccessorConfiguration> : TestsBase<TStartup>
        where TStartup : IStartup, new()
    {
        protected AgentAccessorTestsBase(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
        {
        }

        protected abstract void ConfigureInactiveAgentAccessor(IServiceCollection serviceCollection);

        protected abstract void ConfigureActiveAgentAccessor(IServiceCollection serviceCollection, TAgentAccessorConfiguration agentAccessorConfiguration);

        protected abstract TAgentAccessorConfiguration[] GetAgentAccessorConfigurations();

        protected abstract TAgentAccessorConfiguration GetAgentAccessorConfigurationWithRole(string role);

        protected abstract TAgentAccessorConfiguration GetAgentAccessorConfigurationWithoutRole(string role);

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
            foreach (var agentAccessorConfiguration in GetAgentAccessorConfigurations())
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
        public void GivenBackingServiceActive_WhenGettingAgentSignature_ThenReturnAgentSignature()
        {
            foreach (var agentAccessorConfiguration in GetAgentAccessorConfigurations())
            {
                // ARRANGE

                using var serviceScope = CreateServiceScope(serviceCollection =>
                {
                    ConfigureActiveAgentAccessor(serviceCollection, agentAccessorConfiguration);
                });

                var agentAccessor = serviceScope.ServiceProvider
                    .GetRequiredService<IAgentAccessor>();

                // ACT

                var agentSignature = agentAccessor.GetAgent().GetSignature();

                // ASSERT

                agentSignature.ShouldNotBeNull();
            }
        }

        [Fact]
        public void GivenAgentWithRole_WhenCheckingIfAgentHasRole_ThenReturnTrue()
        {
            // ARRANGE

            const string role = "TestRole";

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, GetAgentAccessorConfigurationWithRole(role));
            });

            var agentAccessor = serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>();

            // ACT

            var hasRole = agentAccessor.GetAgent().HasRole(role);

            // ASSERT

            hasRole.ShouldBeTrue();
        }

        [Fact]
        public void GivenAgentWithoutRole_WhenCheckingIfAgentHasRole_ThenReturnFalse()
        {
            // ARRANGE

            const string role = "TestRole";

            using var serviceScope = CreateServiceScope(serviceCollection =>
            {
                ConfigureActiveAgentAccessor(serviceCollection, GetAgentAccessorConfigurationWithoutRole(role));
            });

            var agentAccessor = serviceScope.ServiceProvider
                .GetRequiredService<IAgentAccessor>();

            // ACT

            var hasRole = agentAccessor.GetAgent().HasRole(role);

            // ASSERT

            hasRole.ShouldBeFalse();
        }
    }
}
