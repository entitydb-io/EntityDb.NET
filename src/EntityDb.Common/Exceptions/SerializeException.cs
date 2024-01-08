namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an object envelope cannot be serialized. Possible objects include:
///     agentSignatures, deltas, leases, tags, and aliases.
/// </summary>
public sealed class SerializeException : Exception
{
}
