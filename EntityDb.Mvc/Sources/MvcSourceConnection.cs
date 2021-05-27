using Microsoft.AspNetCore.Http;

namespace EntityDb.Mvc.Sources
{
    /// <summary>
    /// Represents the connection used by agent to request a transaction using a <see cref="Microsoft.AspNetCore.Mvc.Controller"/>.
    /// </summary>
    public sealed record MvcSourceConnection(string ConnectionId, string? RemoteIpAddress, int RemotePort, string? LocalIpAddress, int LocalPort)
    {
        /// <summary>
        /// Returns a new instance of <see cref="MvcSourceConnection"/> that is mapped from an <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
        /// <returns>A new instance of <see cref="MvcSourceConnection"/> that is mapped from <paramref name="httpContext"/>.</returns>
        public static MvcSourceConnection FromHttpContext(HttpContext httpContext)
        {
            return new MvcSourceConnection
            (
                ConnectionId: httpContext.Connection.Id,
                RemoteIpAddress: httpContext.Connection.RemoteIpAddress?.ToString(),
                RemotePort: httpContext.Connection.RemotePort,
                LocalIpAddress: httpContext.Connection.LocalIpAddress?.ToString(),
                LocalPort: httpContext.Connection.LocalPort
            );
        }
    }
}
