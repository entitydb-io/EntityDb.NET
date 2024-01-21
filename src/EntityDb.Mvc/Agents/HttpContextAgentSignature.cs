using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace EntityDb.Mvc.Agents;

/// <summary>
///     Represents the description of an agent who records sources using an
///     <see cref="HttpContext" />.
/// </summary>
public sealed class HttpContextAgentSignature
{
    /// <summary>
    ///     Request details
    /// </summary>
    public required RequestSnapshot Request { get; init; }

    /// <summary>
    ///     Connection details
    /// </summary>
    public required ConnectionSnapshot Connection { get; init; }

    /// <summary>
    ///     Application details
    /// </summary>
    public required Dictionary<string, string> ApplicationInfo { get; init; }

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

    internal static HttpContextAgentSignature GetSnapshot
    (
        HttpContext httpContext,
        HttpContextAgentSignatureOptions httpContextAgentOptions,
        Dictionary<string, string> applicationInfo
    )
    {
        return new HttpContextAgentSignature
        {
            Request = GetRequestSnapshot(httpContext.Request, httpContextAgentOptions),
            Connection = GetConnectionSnapshot(httpContext.Connection),
            ApplicationInfo = applicationInfo,
        };
    }


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
    ///     Represents the headers used by agent.
    /// </summary>
    public sealed record NameValuesPairSnapshot
    (
        string Name,
        string?[] Values
    );
}
