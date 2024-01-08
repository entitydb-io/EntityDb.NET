using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;
using Microsoft.Extensions.Logging;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an actor passes an
///     <see cref="Source" /> to
///     <see cref="ISourceRepository.Commit" /> with any
///     <see cref="Message" /> where the value of
///     <see cref="Pointer.Version" /> in
///     <see cref="Message.EntityPointer" />
///     is not equal to <see cref="Abstractions.ValueObjects.Version.Next()" />
///     of the committed previous version.
/// </summary>
/// <remarks>
///     A program will not be able to catch this exception if it is thrown.
///     <see cref="ISourceRepository.Commit" /> will return false, and this
///     exception will be logged using the injected <see cref="ILogger{TCategoryName}" />.
/// </remarks>
public sealed class OptimisticConcurrencyException : Exception
{
    /// <summary>
    ///     Throws a new <see cref="OptimisticConcurrencyException" /> if <paramref name="actualPreviousVersion" />
    ///     is not equal to <paramref name="expectedPreviousVersion" />.
    /// </summary>
    /// <param name="expectedPreviousVersion">The expected previous version.</param>
    /// <param name="actualPreviousVersion">The actual previous version.</param>
    public static void ThrowIfMismatch(Version expectedPreviousVersion,
        Version actualPreviousVersion)
    {
        if (expectedPreviousVersion != actualPreviousVersion)
        {
            throw new OptimisticConcurrencyException();
        }
    }
}
