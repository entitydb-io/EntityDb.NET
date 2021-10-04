using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Envelopes;
using EntityDb.Mvc.Sources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using Xunit;

namespace EntityDb.Mvc.Tests.Sources
{
    public class MvcSourceTests
    {
        private readonly ILogger _logger;
        private readonly IResolvingStrategyChain _resolvingStrategyChain;

        public MvcSourceTests(ILoggerFactory loggerFactory, IResolvingStrategyChain resolvingStrategyChain)
        {
            _logger = loggerFactory.CreateLogger<MvcSourceTests>();
            _resolvingStrategyChain = resolvingStrategyChain;
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
            Dictionary<string, StringValues>? headers = new Dictionary<string, StringValues>
            {
                [headerName] = new(headerValue)
            };

            ClaimsPrincipal? claimsPrincipal = new();
            ClaimsIdentity? claimsIdentity = new();
            Claim? claim = new(claimType, claimValue);

            claimsIdentity.AddClaim(claim);
            claimsPrincipal.AddIdentity(claimsIdentity);

            Mock<IHeaderDictionary>? headerDictionaryMock = new(MockBehavior.Strict);

            headerDictionaryMock
                .Setup(dictionary => dictionary.GetEnumerator())
                .Returns(headers.GetEnumerator());

            Mock<HttpRequest>? httpRequestMock = new(MockBehavior.Strict);

            httpRequestMock
                .SetupGet(request => request.Headers)
                .Returns(headerDictionaryMock.Object);

            Mock<ConnectionInfo>? connectionInfoMock = new(MockBehavior.Strict);

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

            Mock<HttpContext>? httpContextMock = new(MockBehavior.Strict);

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
        public void GivenHttpContext_ThenBuildMvcSource
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

            HttpContext? httpContext = CreateHttpContext(headerName, headerValue, connectionId, remoteIpAddress,
                remotePort, localIpAddress, localPort, claimType, claimValue);

            // ACT

            MvcSource? mvcSource = MvcSource.FromHttpContext(httpContext);

            // ASSERT

            mvcSource.Headers.Length.ShouldBe(1);
            mvcSource.Headers[0].Name.ShouldBe(headerName);
            mvcSource.Headers[0].Values.Length.ShouldBe(1);
            mvcSource.Headers[0].Values[0].ShouldBe(headerValue);

            mvcSource.Connection.ConnectionId.ShouldBe(connectionId);
            mvcSource.Connection.RemoteIpAddress.ShouldBe(remoteIpAddress);
            mvcSource.Connection.RemotePort.ShouldBe(remotePort);
            mvcSource.Connection.LocalIpAddress.ShouldBe(localIpAddress);
            mvcSource.Connection.LocalPort.ShouldBe(localPort);
        }

        [Theory]
        [InlineData("Content-Type", "application/json", "", "127.0.0.1", 80, "127.0.0.1", 80, ClaimTypes.Role,
            "TestRole")]
        [InlineData("Content-Type", "application/json", "", null, 80, null, 80, ClaimTypes.Role, "TestRole")]
        public void CanDeconstructMvcSourceAsBsonDocumentEnvelope
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

            HttpContext? httpContext = CreateHttpContext(headerName, headerValue, connectionId, remoteIpAddress,
                remotePort, localIpAddress, localPort, claimType, claimValue);

            MvcSource? originalMvcSource = MvcSource.FromHttpContext(httpContext);

            BsonDocumentEnvelope? bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(originalMvcSource, _logger);

            // ACT

            MvcSource? reconstructedMvcSource =
                bsonDocumentEnvelope.Reconstruct<MvcSource>(_logger, _resolvingStrategyChain);

            // ASSERT

            reconstructedMvcSource.Headers.Length.ShouldBe(1);
            reconstructedMvcSource.Headers[0].Name.ShouldBe(headerName);
            reconstructedMvcSource.Headers[0].Values.Length.ShouldBe(1);
            reconstructedMvcSource.Headers[0].Values[0].ShouldBe(headerValue);

            reconstructedMvcSource.Connection.ConnectionId.ShouldBe(connectionId);
            reconstructedMvcSource.Connection.RemoteIpAddress.ShouldBe(remoteIpAddress);
            reconstructedMvcSource.Connection.RemotePort.ShouldBe(remotePort);
            reconstructedMvcSource.Connection.LocalIpAddress.ShouldBe(localIpAddress);
            reconstructedMvcSource.Connection.LocalPort.ShouldBe(localPort);
        }
    }
}
