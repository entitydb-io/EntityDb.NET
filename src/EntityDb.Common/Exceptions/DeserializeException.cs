using System;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an object envelope cannot be deserialized. Possible objects include:
///     agentSignatures,
///     commands, facts, and leases.
/// </summary>
public sealed class DeserializeException : Exception
{
}
