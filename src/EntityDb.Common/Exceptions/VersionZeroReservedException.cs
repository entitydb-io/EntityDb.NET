using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using System;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an actor passes an <see cref="ITransaction" /> to
///     <see cref="ITransactionRepository.PutTransaction(ITransaction)" /> with any
///     <see cref="ICommandTransactionStep.NextEntityVersionNumber" /> equal to zero.
/// </summary>
/// <remarks>
///     Version Zero is reserved for an entity that has not yet been created/persisted.
/// </remarks>
public class VersionZeroReservedException : Exception
{
    /// <summary>
    ///     Throws a new <see cref="VersionZeroReservedException" /> if <paramref name="versionNumber" /> is
    ///     equal to zero.
    /// </summary>
    /// <param name="versionNumber"></param>
    public static void ThrowIfZero(ulong versionNumber)
    {
        if (versionNumber == 0)
        {
            throw new VersionZeroReservedException();
        }
    }
}
