using EntityDb.Common.Tests.Agents;
using EntityDb.Mvc.Tests.Seeder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace EntityDb.Mvc.Tests.Agents
{
    public class HttpContextAgentAccessorTests : AgentAccessorTestsBase<Startup, HttpContextSeederOptions>
    {
        public HttpContextAgentAccessorTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
        {
        }

        protected override void ConfigureInactiveAgentAccessor(IServiceCollection serviceCollection)
        {
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>(MockBehavior.Strict);

            httpContextAccessorMock
                .SetupGet(accessor => accessor.HttpContext)
                .Returns(default(HttpContext?));

            serviceCollection.AddSingleton(httpContextAccessorMock.Object);
        }

        protected override void ConfigureActiveAgentAccessor(IServiceCollection serviceCollection, HttpContextSeederOptions httpContextSeederOptions)
        {
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>(MockBehavior.Strict);

            httpContextAccessorMock
                .SetupGet(accessor => accessor.HttpContext)
                .Returns(HttpContextSeeder.CreateHttpContext(httpContextSeederOptions));

            serviceCollection.AddSingleton(httpContextAccessorMock.Object);
        }

        protected override HttpContextSeederOptions[] GetAgentAccessorConfigurations()
        {
            return new[]
            {
                new HttpContextSeederOptions
                {
                    HasIpAddress = true,
                },
                new HttpContextSeederOptions
                {
                    HasIpAddress = false,
                },
            };
        }

        protected override HttpContextSeederOptions GetAgentAccessorConfigurationWithRole(string role)
        {
            return new HttpContextSeederOptions
            {
                Role = role
            };
        }

        protected override HttpContextSeederOptions GetAgentAccessorConfigurationWithoutRole(string role)
        {
            return new HttpContextSeederOptions
            {
                Role = $"Not{role}"
            };
        }
    }
}
