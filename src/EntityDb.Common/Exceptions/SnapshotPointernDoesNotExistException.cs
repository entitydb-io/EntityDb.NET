using System;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an actor requests a snapshot that does not exist.
/// </summary>
public sealed class SnapshotPointernDoesNotExistException : Exception
{
}
