namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an object envelope cannot be deserialized. Possible objects include:
///     agentSignatures, deltas, facts, leases, and aliases.
/// </summary>
public sealed class DeserializeException : Exception
{
}
