using EntityDb.Abstractions.Sources;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an actor passes a <see cref="Source" /> to an
///     <see cref="ISourceRepository" /> that was created for read-only mode.
/// </summary>
public sealed class ReadOnlyWriteException : Exception;
