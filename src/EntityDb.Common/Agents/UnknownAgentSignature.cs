namespace EntityDb.Common.Agents;

/// <summary>
///     Represents the signature of an unknown actor.
/// </summary>
/// <param name="ApplicationInfo">A snapshot of the application information</param>
public record UnknownAgentSignature(Dictionary<string, string> ApplicationInfo);
