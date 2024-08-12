using EntityDb.Abstractions.Streams;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when <see cref="IMultipleStreamRepository.LoadOrCreate" />
///     is called for a stream that already exists in the repository.
/// </summary>
public sealed class ExistingStreamException : Exception;
