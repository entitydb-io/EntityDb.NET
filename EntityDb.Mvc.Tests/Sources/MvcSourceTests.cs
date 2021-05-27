using EntityDb.MongoDb.Envelopes;
using EntityDb.Mvc.Sources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using Xunit;

namespace EntityDb.Mvc.Tests.Sources
{
    public class MvcSourceTests
    {
        private readonly IServiceProvider _serviceProvider;

        public MvcSourceTests(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private static MvcSource CreateMvcSource()
        {
            var headers = new[]
            {
                new MvcSourceHeader("Test", new[]{ "Test" }),
            };

            var connection = new MvcSourceConnection("Test", "http://test.remote", 123, "http://test.local", 456);

            var claims = new[]
            {
                new MvcSourceClaim("Role", "Test", null, "", ""),
            };

            return new MvcSource(headers, connection, claims);
        }

        [Theory]
        [InlineData("Content-Type", "application/json", "", "127.0.0.1", 80, "127.0.0.1", 80, ClaimTypes.Role, "TestRole")]
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

            var headers = new Dictionary<string, StringValues>
            {
                [headerName] = new StringValues(headerValue),
            };

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

            var httpContext = httpContextMock.Object;

            // ACT

            var mvcSource = MvcSource.FromHttpContext(httpContext);

            // ASSERT

            Assert.Single(mvcSource.Headers);
            Assert.Equal(headerName, mvcSource.Headers[0].Name);
            Assert.Single(mvcSource.Headers[0].Values);
            Assert.Equal(headerValue, mvcSource.Headers[0].Values[0]);

            Assert.Equal(connectionId, mvcSource.Connection.ConnectionId);
            Assert.Equal(remoteIpAddress, mvcSource.Connection.RemoteIpAddress);
            Assert.Equal(remotePort, mvcSource.Connection.RemotePort);
            Assert.Equal(localIpAddress, mvcSource.Connection.LocalIpAddress);
            Assert.Equal(localPort, mvcSource.Connection.LocalPort);

            Assert.Single(mvcSource.Claims);
            Assert.Equal(claimType, mvcSource.Claims[0].Type);
            Assert.Equal(claimValue, mvcSource.Claims[0].Value);
        }

        [Fact]
        public void CanDeconstructMvcSourceAsBsonDocumentEnvelope()
        {
            var originalMvcSource = CreateMvcSource();

            var bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(originalMvcSource, _serviceProvider);

            var reconstructedMvcSource = bsonDocumentEnvelope.Reconstruct<MvcSource>(_serviceProvider);

            Assert.Single(reconstructedMvcSource.Headers);
            Assert.Single(reconstructedMvcSource.Headers[0].Values);
            Assert.Equal("Test", reconstructedMvcSource.Headers[0].Name);
            Assert.Equal("Test", reconstructedMvcSource.Headers[0].Values[0]);

            Assert.Equal("Test", reconstructedMvcSource.Connection.ConnectionId);
            Assert.Equal("http://test.remote", reconstructedMvcSource.Connection.RemoteIpAddress);
            Assert.Equal(123, reconstructedMvcSource.Connection.RemotePort);
            Assert.Equal("http://test.local", reconstructedMvcSource.Connection.LocalIpAddress);
            Assert.Equal(456, reconstructedMvcSource.Connection.LocalPort);

            Assert.Single(reconstructedMvcSource.Claims);
            Assert.Equal("Role", reconstructedMvcSource.Claims[0].Type);
            Assert.Equal("Test", reconstructedMvcSource.Claims[0].Value);
            Assert.Null(reconstructedMvcSource.Claims[0].ValueType);
            Assert.Equal("", reconstructedMvcSource.Claims[0].Issuer);
            Assert.Equal("", reconstructedMvcSource.Claims[0].OriginalIssuer);
        }
    }
}
