using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using Microsoft.Extensions.Logging;
using System;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is logged when an actor passes a <see cref="ITransaction" /> to an
///     <see cref="ITransactionRepository" /> with a
///     <see cref="IAppendCommandTransactionStep.PreviousEntityVersionNumber" /> that is not the actual
///     previous version number.
/// </summary>
/// <remarks>
///     A program will not be able to catch this exception if it is thrown.
///     <see cref="ITransactionRepository.PutTransaction(ITransaction)" /> will return false, and this
///     exception will be logged using the injected <see cref="ILogger" />.
/// </remarks>
public sealed class OptimisticConcurrencyException : Exception
{
    /// <summary>
    ///     Throws a new <see cref="OptimisticConcurrencyException" /> if <paramref name="actualPreviousVersionNumber" />
    ///     is not equal to <paramref name="expectedPreviousVersionNumber" />.
    /// </summary>
    /// <param name="expectedPreviousVersionNumber">The expected previous version number.</param>
    /// <param name="actualPreviousVersionNumber">The actual previous version number.</param>
    public static void ThrowIfMismatch(VersionNumber expectedPreviousVersionNumber, VersionNumber actualPreviousVersionNumber)
    {
        if (expectedPreviousVersionNumber != actualPreviousVersionNumber)
        {
            throw new OptimisticConcurrencyException();
        }
    }
}
