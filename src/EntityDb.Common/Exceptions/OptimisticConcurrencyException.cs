using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.States;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an actor passes an
///     <see cref="Source" /> to
///     <see cref="ISourceRepository.Commit" /> with any
///     <see cref="Message" /> where the value of
///     <see cref="StatePointer.StateVersion" /> in
///     <see cref="Message.StatePointer" />
///     is not equal to <see cref="StateVersion.Next()" />
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
    ///     Throws a new <see cref="OptimisticConcurrencyException" /> if <paramref name="actualPreviousStateVersion" />
    ///     is not equal to <paramref name="expectedPreviousStateVersion" />.
    /// </summary>
    /// <param name="expectedPreviousStateVersion">The expected previous version.</param>
    /// <param name="actualPreviousStateVersion">The actual previous version.</param>
    public static void ThrowIfMismatch(StateVersion expectedPreviousStateVersion,
        StateVersion actualPreviousStateVersion)
    {
        if (expectedPreviousStateVersion != actualPreviousStateVersion)
        {
            throw new OptimisticConcurrencyException();
        }
    }
}
