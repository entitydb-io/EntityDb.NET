using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Extensions;
using EntityDb.Common.Tests;
using EntityDb.MongoDb.Envelopes;
using EntityDb.Mvc.Agents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using Xunit;

namespace EntityDb.Mvc.Tests.Agents
{
    public class HttpContextAgentSignatureTests : TestsBase<Startup>
    {
        public HttpContextAgentSignatureTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
        {
        }

        private static HttpContext CreateHttpContext
        (
            string headerName,
            string headerValue,
            string connectionId,
            string? remoteIpAddress,
            int remotePort,
            string? localIpAddress,
            int localPort,
            string claimType,
            string claimValue
        )
        {
            var headers = new Dictionary<string, StringValues> { [headerName] = new(headerValue) };

            var claimsPrincipal = new ClaimsPrincipal();
            var claimsIdentity = new ClaimsIdentity();
            var claim = new Claim(claimType, claimValue);

            claimsIdentity.AddClaim(claim);
            claimsPrincipal.AddIdentity(claimsIdentity);

            var headerDictionaryMock = new Mock<IHeaderDictionary>(MockBehavior.Strict);

            headerDictionaryMock
                .Setup(dictionary => dictionary.GetEnumerator())
                .Returns(headers.GetEnumerator());

            var httpRequestMock = new Mock<HttpRequest>(MockBehavior.Strict);

            httpRequestMock
                .SetupGet(request => request.Headers)
                .Returns(headerDictionaryMock.Object);

            var connectionInfoMock = new Mock<ConnectionInfo>(MockBehavior.Strict);

            connectionInfoMock
                .SetupGet(info => info.Id)
                .Returns(connectionId);

            connectionInfoMock
                .SetupGet(info => info.RemoteIpAddress)
                .Returns(remoteIpAddress == null ? null : IPAddress.Parse(remoteIpAddress));

            connectionInfoMock
                .SetupGet(info => info.RemotePort)
                .Returns(remotePort);

            connectionInfoMock
                .SetupGet(info => info.LocalIpAddress)
                .Returns(localIpAddress == null ? null : IPAddress.Parse(localIpAddress));

            connectionInfoMock
                .SetupGet(info => info.LocalPort)
                .Returns(localPort);

            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);

            httpContextMock
                .SetupGet(context => context.Request)
                .Returns(httpRequestMock.Object);

            httpContextMock
                .SetupGet(context => context.Connection)
                .Returns(connectionInfoMock.Object);

            httpContextMock
                .SetupGet(context => context.User)
                .Returns(claimsPrincipal);

            return httpContextMock.Object;
        }

        [Theory]
        [InlineData("Content-Type", "application/json", "", "127.0.0.1", 80, "127.0.0.1", 80, ClaimTypes.Role,
            "TestRole")]
        [InlineData("Content-Type", "application/json", "", null, 80, null, 80, ClaimTypes.Role, "TestRole")]
        public void GivenHttpContext_ThenBuildHttpContextAgentSignature
        (
            string headerName,
            string headerValue,
            string connectionId,
            string? remoteIpAddress,
            int remotePort,
            string? localIpAddress,
            int localPort,
            string claimType,
            string claimValue
        )
        {
            // ARRANGE

            var httpContext = CreateHttpContext(headerName, headerValue, connectionId, remoteIpAddress, remotePort,
                localIpAddress, localPort, claimType, claimValue);

            // ACT

            var agentSignature = HttpContextAgentSignature.FromHttpContext(httpContext);

            // ASSERT

            agentSignature.Headers.Length.ShouldBe(1);
            agentSignature.Headers[0].Name.ShouldBe(headerName);
            agentSignature.Headers[0].Values.Length.ShouldBe(1);
            agentSignature.Headers[0].Values[0].ShouldBe(headerValue);

            agentSignature.Connection.ConnectionId.ShouldBe(connectionId);
            agentSignature.Connection.RemoteIpAddress.ShouldBe(remoteIpAddress);
            agentSignature.Connection.RemotePort.ShouldBe(remotePort);
            agentSignature.Connection.LocalIpAddress.ShouldBe(localIpAddress);
            agentSignature.Connection.LocalPort.ShouldBe(localPort);
        }

        [Theory]
        [InlineData("Content-Type", "application/json", "", "127.0.0.1", 80, "127.0.0.1", 80, ClaimTypes.Role,
            "TestRole")]
        [InlineData("Content-Type", "application/json", "", null, 80, null, 80, ClaimTypes.Role, "TestRole")]
        public void CanDeconstructMvcAgentSignatureAsBsonDocumentEnvelope
        (
            string headerName,
            string headerValue,
            string connectionId,
            string? remoteIpAddress,
            int remotePort,
            string? localIpAddress,
            int localPort,
            string claimType,
            string claimValue
        )
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var logger = serviceScope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<HttpContextAgentSignatureTests>();

            var resolvingStategyChain = serviceScope.ServiceProvider
                .GetRequiredService<IResolvingStrategyChain>();

            var httpContext = CreateHttpContext(headerName, headerValue, connectionId, remoteIpAddress, remotePort,
                localIpAddress, localPort, claimType, claimValue);

            var originalHttpContextAgentSignature = HttpContextAgentSignature.FromHttpContext(httpContext);

            var bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(originalHttpContextAgentSignature, logger);

            // ACT

            var reconstructedHttpContextAgentSignature = bsonDocumentEnvelope.Reconstruct<HttpContextAgentSignature>(logger, resolvingStategyChain);

            // ASSERT

            reconstructedHttpContextAgentSignature.Headers.Length.ShouldBe(1);
            reconstructedHttpContextAgentSignature.Headers[0].Name.ShouldBe(headerName);
            reconstructedHttpContextAgentSignature.Headers[0].Values.Length.ShouldBe(1);
            reconstructedHttpContextAgentSignature.Headers[0].Values[0].ShouldBe(headerValue);

            reconstructedHttpContextAgentSignature.Connection.ConnectionId.ShouldBe(connectionId);
            reconstructedHttpContextAgentSignature.Connection.RemoteIpAddress.ShouldBe(remoteIpAddress);
            reconstructedHttpContextAgentSignature.Connection.RemotePort.ShouldBe(remotePort);
            reconstructedHttpContextAgentSignature.Connection.LocalIpAddress.ShouldBe(localIpAddress);
            reconstructedHttpContextAgentSignature.Connection.LocalPort.ShouldBe(localPort);
        }
    }
}
