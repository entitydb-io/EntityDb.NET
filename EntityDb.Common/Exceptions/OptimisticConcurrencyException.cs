using EntityDb.Abstractions.Transactions;
using Microsoft.Extensions.Logging;
using System;

namespace EntityDb.Common.Exceptions
{
    /// <summary>
    /// The exception that is logged when an actor passes a <see cref="ITransaction{TEntity}"/> to an <see cref="ITransactionRepository{TEntity}"/> with a <see cref="ITransactionCommand{TEntity}.ExpectedPreviousVersionNumber"/> unequal to the actual previous version number.
    /// </summary>
    /// <remarks>
    /// A program will not be able to catch this exception if it is thrown. <see cref="ITransactionRepository{TEntity}.PutTransaction(ITransaction{TEntity})"/> will return false, and this exception will be logged using the injected <see cref="ILogger"/>.
    /// </remarks>
    //TODO: Add a test to verify that the implementation does exactly this.
    public sealed class OptimisticConcurrencyException : Exception
    {
    }
}
