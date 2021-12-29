using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Mvc.Agents
{
    /// <summary>
    ///     Represents the description of an agent who requests transactions using an
    ///     <see cref="HttpContext" />.
    /// </summary>
    public static class HttpContextAgentSignature
    {
        /// <summary>
        ///     Represents the headers used by agent.
        /// </summary>
        public sealed record NameValuesPairSnapshot
        (
            string Name,
            string[] Values
        );

        /// <summary>
        ///     Represents the request sent by the agent.
        /// </summary>
        public sealed record RequestSnapshot
        (
            string Method,
            string Scheme,
            string Host,
            string? Path,
            string Protocol,
            NameValuesPairSnapshot[] Headers,
            NameValuesPairSnapshot[] Queries
        );

        /// <summary>
        ///     Represents the connection used by agent.
        /// </summary>
        public sealed record ConnectionSnapshot
        (
            string ConnectionId,
            string? RemoteIpAddress,
            int RemotePort,
            string? LocalIpAddress,
            int LocalPort
        );

        /// <summary>
        ///     Represents the signature of the agent.
        /// </summary>
        public sealed record Snapshot
        (
            RequestSnapshot Request,
            ConnectionSnapshot Connection
        );

        private static NameValuesPairSnapshot[] GetNameValuesPairSnapshots(IEnumerable<KeyValuePair<string, StringValues>> dictionary, HttpContextAgentSignatureOptions httpContextAgentOptions)
        {
            return dictionary
                .Where(pair => !httpContextAgentOptions.DoNotRecordHeaders.Contains(pair.Key))
                .Select(pair => new NameValuesPairSnapshot
                (
                    Name: pair.Key,
                    Values: pair.Value.ToArray()
                ))
                .ToArray();
        }

        private static RequestSnapshot GetRequestSnapshot(HttpRequest httpRequest, HttpContextAgentSignatureOptions httpContextAgentOptions)
        {
            return new RequestSnapshot
            (
                Method: httpRequest.Method,
                Scheme: httpRequest.Scheme,
                Host: httpRequest.Host.Value,
                Path: httpRequest.Path.Value,
                Protocol: httpRequest.Protocol,
                Headers: GetNameValuesPairSnapshots(httpRequest.Headers, httpContextAgentOptions),
                Queries: GetNameValuesPairSnapshots(httpRequest.Query, httpContextAgentOptions)
            );
        }

        private static ConnectionSnapshot GetConnectionSnapshot(ConnectionInfo connection)
        {
            return new ConnectionSnapshot
            (
                ConnectionId: connection.Id,
                RemoteIpAddress: connection.RemoteIpAddress?.ToString(),
                RemotePort: connection.RemotePort,
                LocalIpAddress: connection.LocalIpAddress?.ToString(),
                LocalPort: connection.LocalPort
            );
        }

        internal static Snapshot GetSnapshot(HttpContext httpContext, HttpContextAgentSignatureOptions httpContextAgentOptions)
        {
            return new Snapshot
            (
                Request: GetRequestSnapshot(httpContext.Request, httpContextAgentOptions),
                Connection: GetConnectionSnapshot(httpContext.Connection)
            );
        }
    }
}
