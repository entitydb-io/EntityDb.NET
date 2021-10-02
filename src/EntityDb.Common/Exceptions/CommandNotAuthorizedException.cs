using EntityDb.Common.Transactions;
using System;

namespace EntityDb.Common.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an actor executes an unauthorized commaind when using <see cref="TransactionBuilder{TEntity}"/>.
    /// </summary>
    public sealed class CommandNotAuthorizedException : Exception
    {
    }
}
