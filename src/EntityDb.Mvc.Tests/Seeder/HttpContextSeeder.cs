using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace EntityDb.Mvc.Tests.Seeder
{
    public static class HttpContextSeeder
    {
        private static ConnectionInfo CreateConnectionInfo(HttpContextSeederOptions httpContextSeederOptions)
        {
            var connectionInfoMock = new Mock<ConnectionInfo>(MockBehavior.Strict);

            connectionInfoMock
                .SetupGet(info => info.Id)
                .Returns(Guid.NewGuid().ToString());

            var faker = new Faker();

            connectionInfoMock
                .SetupGet(info => info.RemoteIpAddress)
                .Returns(httpContextSeederOptions.HasIpAddress ? faker.Internet.IpAddress() : null);

            connectionInfoMock
                .SetupGet(info => info.RemotePort)
                .Returns(faker.Internet.Port());

            connectionInfoMock
                .SetupGet(info => info.LocalIpAddress)
                .Returns(httpContextSeederOptions.HasIpAddress ? faker.Internet.IpAddress() : null);

            connectionInfoMock
                .SetupGet(info => info.LocalPort)
                .Returns(faker.Internet.Port());

            return connectionInfoMock.Object;
        }

        private static HttpRequest CreateHttpRequest()
        {
            var headers = new Dictionary<string, StringValues>
            {
                ["Content-Type"] = new("application/json"),
            };

            var headerDictionaryMock = new Mock<IHeaderDictionary>(MockBehavior.Strict);

            headerDictionaryMock
                .Setup(dictionary => dictionary.GetEnumerator())
                .Returns(headers.GetEnumerator());

            var httpRequestMock = new Mock<HttpRequest>(MockBehavior.Strict);

            httpRequestMock
                .SetupGet(request => request.Headers)
                .Returns(headerDictionaryMock.Object);

            return httpRequestMock.Object;
        }

        private static ClaimsPrincipal CreateClaimsPrincipal(HttpContextSeederOptions httpContextSeederOptions)
        {
            var claimsPrincipal = new ClaimsPrincipal();
            var claimsIdentity = new ClaimsIdentity();

            if (httpContextSeederOptions.Role != null)
            {
                var claim = new Claim(ClaimTypes.Role, httpContextSeederOptions.Role);

                claimsIdentity.AddClaim(claim);
            }

            claimsPrincipal.AddIdentity(claimsIdentity);

            return claimsPrincipal;
        }

        public static HttpContext CreateHttpContext(HttpContextSeederOptions httpContextSeederOptions)
        {
            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);

            httpContextMock
                .SetupGet(context => context.Request)
                .Returns(CreateHttpRequest());

            httpContextMock
                .SetupGet(context => context.Connection)
                .Returns(CreateConnectionInfo(httpContextSeederOptions));

            httpContextMock
                .SetupGet(context => context.User)
                .Returns(CreateClaimsPrincipal(httpContextSeederOptions));

            return httpContextMock.Object;
        }
    }
}
