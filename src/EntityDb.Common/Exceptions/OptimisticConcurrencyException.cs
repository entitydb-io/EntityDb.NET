using EntityDb.Abstractions.Transactions;
using Microsoft.Extensions.Logging;
using System;

namespace EntityDb.Common.Exceptions
{
    /// <summary>
    ///     The exception that is logged when an actor passes a <see cref="ITransaction{TEntity}" /> to an
    ///     <see cref="ITransactionRepository{TEntity}" /> with a
    ///     <see cref="ITransactionCommand{TEntity}.EntityVersionNumber" /> that is not the next number
    ///     after the previous version number.
    /// </summary>
    /// <remarks>
    ///     A program will not be able to catch this exception if it is thrown.
    ///     <see cref="ITransactionRepository{TEntity}.PutTransaction(ITransaction{TEntity})" /> will return false, and this
    ///     exception will be logged using the injected <see cref="ILogger" />.
    /// </remarks>
    public sealed class OptimisticConcurrencyException : Exception
    {
        /// <summary>
        ///     Throws a new <see cref="OptimisticConcurrencyException" /> if <paramref name="nextVersionNumber" />
        ///     is not the next number after <paramref name="previousVersionNumber" />.
        /// </summary>
        /// <param name="previousVersionNumber">The previous version number.</param>
        /// <param name="nextVersionNumber">The next version number.</param>
        public static void ThrowIfDiscontinuous(ulong previousVersionNumber, ulong nextVersionNumber)
        {
            if (nextVersionNumber != previousVersionNumber + 1)
            {
                throw new OptimisticConcurrencyException();
            }
        }
    }
}
