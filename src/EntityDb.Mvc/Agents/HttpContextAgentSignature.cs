using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Mvc.Agents;

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
        NameValuesPairSnapshot[] QueryStringParams
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

    private static NameValuesPairSnapshot[] GetNameValuesPairSnapshots(IEnumerable<KeyValuePair<string, StringValues>> dictionary, string[] redactedKeys, string redactedValue)
    {
        return dictionary
            .Select(pair => new NameValuesPairSnapshot
            (
                pair.Key,
                redactedKeys.Contains(pair.Key) ? new[] { redactedValue } : pair.Value.ToArray()
            ))
            .ToArray();
    }

    private static RequestSnapshot GetRequestSnapshot(HttpRequest httpRequest, HttpContextAgentSignatureOptions httpContextAgentOptions)
    {
        return new RequestSnapshot
        (
            httpRequest.Method,
            httpRequest.Scheme,
            httpRequest.Host.Value,
            httpRequest.Path.Value,
            httpRequest.Protocol,
            GetNameValuesPairSnapshots(httpRequest.Headers, httpContextAgentOptions.RedactedHeaders, httpContextAgentOptions.RedactedValue),
            GetNameValuesPairSnapshots(httpRequest.Query, httpContextAgentOptions.RedactedQueryStringParams, httpContextAgentOptions.RedactedValue)
        );
    }

    private static ConnectionSnapshot GetConnectionSnapshot(ConnectionInfo connection)
    {
        return new ConnectionSnapshot
        (
            connection.Id,
            connection.RemoteIpAddress?.ToString(),
            connection.RemotePort,
            connection.LocalIpAddress?.ToString(),
            connection.LocalPort
        );
    }

    internal static Snapshot GetSnapshot(HttpContext httpContext, HttpContextAgentSignatureOptions httpContextAgentOptions)
    {
        return new Snapshot
        (
            GetRequestSnapshot(httpContext.Request, httpContextAgentOptions),
            GetConnectionSnapshot(httpContext.Connection)
        );
    }
}
