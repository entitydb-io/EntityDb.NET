using EntityDb.Abstractions.Transactions;
using System;

namespace EntityDb.Common.Exceptions
{
    /// <summary>
    ///     The exception that is thrown when an actor passes a <see cref="ITransaction{TEntity}" /> to an
    ///     <see cref="ITransactionRepository{TEntity}" /> that was created with
    ///     <see cref="ITransactionSessionOptions.ReadOnly" /> equal to <c>true</c>.
    /// </summary>
    public class CannotWriteInReadOnlyModeException : Exception
    {
    }
}
