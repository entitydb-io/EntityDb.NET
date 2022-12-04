using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace EntityDb.Mvc.Agents;

/// <summary>
///     Represents the description of an agent who requests transactions using an
///     <see cref="HttpContext" />.
/// </summary>
public static class HttpContextAgentSignature
{
    private static NameValuesPairSnapshot[] GetNameValuesPairSnapshots(
        IEnumerable<KeyValuePair<string, StringValues>> dictionary, string[] redactedKeys, string redactedValue)
    {
        return dictionary
            .Select(pair => new NameValuesPairSnapshot
            (
                pair.Key,
                redactedKeys.Contains(pair.Key) ? new[] { redactedValue } : pair.Value.ToArray()
            ))
            .ToArray();
    }

    private static RequestSnapshot GetRequestSnapshot(HttpRequest httpRequest,
        HttpContextAgentSignatureOptions httpContextAgentOptions)
    {
        return new RequestSnapshot
        (
            httpRequest.Method,
            httpRequest.Scheme,
            httpRequest.Host.Value,
            httpRequest.Path.Value,
            httpRequest.Protocol,
            GetNameValuesPairSnapshots(httpRequest.Headers, httpContextAgentOptions.RedactedHeaders,
                httpContextAgentOptions.RedactedValue),
            GetNameValuesPairSnapshots(httpRequest.Query, httpContextAgentOptions.RedactedQueryStringParams,
                httpContextAgentOptions.RedactedValue)
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

    internal static Snapshot GetSnapshot
    (
        HttpContext httpContext,
        HttpContextAgentSignatureOptions httpContextAgentOptions,
        Dictionary<string, string> applicationInfo
    )
    {
        return new Snapshot
        (
            GetRequestSnapshot(httpContext.Request, httpContextAgentOptions),
            GetConnectionSnapshot(httpContext.Connection),
            applicationInfo
        );
    }

    /// <summary>
    ///     Represents the headers used by agent.
    /// </summary>
    /// <param name="Name">The name of the pair.</param>
    /// <param name="Values">The values of the pair.</param>
    public sealed record NameValuesPairSnapshot
    (
        string Name,
        string?[] Values
    );

    /// <summary>
    ///     Represents the request sent by the agent.
    /// </summary>
    /// <param name="Method">The request method (e.g., GET, POST, etc.)</param>
    /// <param name="Scheme">The request scheme (e.g., HTTP, HTTPS)</param>
    /// <param name="Host">The request server host (e.g., entitydb.io)</param>
    /// <param name="Path">The request server path (e.g., /)</param>
    /// <param name="Protocol">The request protocol (e.g., HTTP/1.1, HTTP/2)</param>
    /// <param name="Headers">The request headers</param>
    /// <param name="QueryStringParams">The request query string parameters.</param>
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
    /// <param name="ConnectionId">The id of the connection.</param>
    /// <param name="RemoteIpAddress">The IP Address of the client.</param>
    /// <param name="RemotePort">The Port of the client.</param>
    /// <param name="LocalIpAddress">The IP Address of the server.</param>
    /// <param name="LocalPort">The Port of the server.</param>
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
    /// <param name="Request">A snapshot of the request</param>
    /// <param name="Connection">A snapshot of the connection</param>
    /// <param name="ApplicationInfo">A snapshot of the application information</param>
    public sealed record Snapshot
    (
        RequestSnapshot Request,
        ConnectionSnapshot Connection,
        Dictionary<string, string> ApplicationInfo
    );
}
