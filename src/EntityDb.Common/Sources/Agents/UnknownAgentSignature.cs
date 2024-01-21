namespace EntityDb.Common.Sources.Agents;

/// <summary>
///     Represents the signature of an unknown actor.
/// </summary>
public sealed record UnknownAgentSignature(Dictionary<string, string> ApplicationInfo);
