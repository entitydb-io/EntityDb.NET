using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an actor passes an <see cref="ITransaction" /> to
///     <see cref="ITransactionRepository.PutTransaction(ITransaction, CancellationToken)" /> with any
///     <see cref="ITransactionCommand" /> where the value of <see cref="ITransactionCommand.EntityVersionNumber" />
///     is equal to <c>0</c>.
/// </summary>
/// <remarks>
///     Version Zero is reserved for an entity that has not yet been created/persisted.
/// </remarks>
public sealed class VersionZeroReservedException : Exception
{
    /// <summary>
    ///     Throws a new <see cref="VersionZeroReservedException" /> if <paramref name="versionNumber" /> is
    ///     equal to zero.
    /// </summary>
    /// <param name="versionNumber"></param>
    public static void ThrowIfZero(VersionNumber versionNumber)
    {
        if (versionNumber == VersionNumber.MinValue)
        {
            throw new VersionZeroReservedException();
        }
    }
}
