using EntityDb.Abstractions.Streams;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when <see cref="IMultipleStreamRepository.Stage" />
///     is called for an stream that is not loaded into the repository.
/// </summary>
public sealed class UnknownStreamException : Exception;
