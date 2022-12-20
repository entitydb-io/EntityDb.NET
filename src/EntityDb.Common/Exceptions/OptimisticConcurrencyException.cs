using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an actor passes an <see cref="ITransaction" /> to
///     <see cref="ITransactionRepository.PutTransaction(ITransaction, CancellationToken)" /> with any
///     <see cref="ITransactionCommand" /> where the value of <see cref="ITransactionCommand.EntityVersionNumber" />
///     is not equal to <see cref="VersionNumber.Next()"/> of the committed previous version number.
/// </summary>
/// <remarks>
///     A program will not be able to catch this exception if it is thrown.
///     <see cref="ITransactionRepository.PutTransaction(ITransaction, CancellationToken)" /> will return false, and this
///     exception will be logged using the injected <see cref="ILogger{TCategoryName}" />.
/// </remarks>
public sealed class OptimisticConcurrencyException : Exception
{
    /// <summary>
    ///     Throws a new <see cref="OptimisticConcurrencyException" /> if <paramref name="actualPreviousVersionNumber" />
    ///     is not equal to <paramref name="expectedPreviousVersionNumber" />.
    /// </summary>
    /// <param name="expectedPreviousVersionNumber">The expected previous version number.</param>
    /// <param name="actualPreviousVersionNumber">The actual previous version number.</param>
    public static void ThrowIfMismatch(VersionNumber expectedPreviousVersionNumber,
        VersionNumber actualPreviousVersionNumber)
    {
        if (expectedPreviousVersionNumber != actualPreviousVersionNumber)
        {
            throw new OptimisticConcurrencyException();
        }
    }
}
