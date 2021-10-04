using EntityDb.Common.Transactions;
using System;

namespace EntityDb.Common.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an actor executes an unauthorized commaind when using <see cref="TransactionBuilder{TEntity}"/>.
    /// </summary>
    public sealed class CommandNotAuthorizedException : Exception
    {
        /// <summary>
        /// Throws a new <see cref="CommandNotAuthorizedException"/> if <paramref name="isAuthorized"/> is <c>false</c>.
        /// </summary>
        /// <param name="isAuthorized"></param>
        public static void ThrowIfFalse(bool isAuthorized)
        {
            if (isAuthorized == false)
            {
                throw new CommandNotAuthorizedException();
            }
        }
    }
}
