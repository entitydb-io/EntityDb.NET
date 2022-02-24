using System;

namespace EntityDb.Mvc.Agents;

/// <summary>
///     Configuration options for the Http Context agent.
/// </summary>
public class HttpContextAgentSignatureOptions
{
    /// <summary>
    ///     If there is a header whose value is sensitive and should not be recorded in the
    ///     agent signature, put it in this array.
    /// </summary>
    public string[] RedactedHeaders { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     If there is a query string param whose value is sensitive and should not be recorded in the
    ///     agent signature, put it in this array.
    /// </summary>
    public string[] RedactedQueryStringParams { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     Redacted information will be substituted with this value.
    /// </summary>
    public string RedactedValue { get; set; } = "*REDACTED*";
}
