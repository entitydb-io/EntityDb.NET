using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using Microsoft.Extensions.Logging;
using System;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is logged when an actor passes a <see cref="ITransaction{TEntity}" /> to an
///     <see cref="ITransactionRepository{TEntity}" /> with a
///     <see cref="ICommandTransactionStep{TEntity}.PreviousEntityVersionNumber" /> that is not the actual
///     previous version number.
/// </summary>
/// <remarks>
///     A program will not be able to catch this exception if it is thrown.
///     <see cref="ITransactionRepository{TEntity}.PutTransaction(ITransaction{TEntity})" /> will return false, and this
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
    public static void ThrowIfMismatch(ulong expectedPreviousVersionNumber, ulong actualPreviousVersionNumber)
    {
        if (expectedPreviousVersionNumber != actualPreviousVersionNumber)
        {
            throw new OptimisticConcurrencyException();
        }
    }
}
