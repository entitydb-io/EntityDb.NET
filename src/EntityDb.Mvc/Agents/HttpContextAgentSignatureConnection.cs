using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EntityDb.Mvc.Agents
{
    /// <summary>
    ///     Represents the connection used by agent to request a transaction using a
    ///     <see cref="Controller" />.
    /// </summary>
    public sealed record HttpContextAgentSignatureConnection
    {
        /// <summary>
        ///     A unique identifier for the connection.
        /// </summary>
        public string ConnectionId { get; init; } = default!;

        /// <summary>
        ///     The IP address of the remote target.
        /// </summary>
        public string? RemoteIpAddress { get; init; }

        /// <summary>
        ///     The port of the remote target.
        /// </summary>
        public int RemotePort { get; init; }

        /// <summary>
        ///     The IP address of hte local target.
        /// </summary>
        public string? LocalIpAddress { get; init; }

        /// <summary>
        ///     The port of the local target.
        /// </summary>
        public int LocalPort { get; init; }

        /// <summary>
        ///     Returns a new instance of <see cref="HttpContextAgentSignatureConnection" /> that is mapped from an <see cref="HttpContext" />.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext" />.</param>
        /// <returns>A new instance of <see cref="HttpContextAgentSignatureConnection" /> that is mapped from <paramref name="httpContext" />.</returns>
        public static HttpContextAgentSignatureConnection FromHttpContext(HttpContext httpContext)
        {
            return new HttpContextAgentSignatureConnection
            {
                ConnectionId = httpContext.Connection.Id,
                RemoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                RemotePort = httpContext.Connection.RemotePort,
                LocalIpAddress = httpContext.Connection.LocalIpAddress?.ToString(),
                LocalPort = httpContext.Connection.LocalPort
            };
        }
    }
}
